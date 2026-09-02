using System.Globalization;
using System.Reflection;
using AssetCenter.Installation;
using AssetCenter.Sdk;
using Core.Updates;

namespace UI.Services.Updates;

public sealed class AssetCenterAppUpdateService : IAppUpdateService, IDisposable
{
    public const string CompanionAssetId = "cn.dreamness.ra3maputils";
    public const string StableChannel = "stable";
    public const string UpdaterProtocolVersion = "1.0.0";

    private static readonly Uri DistributionEndpoint = new("https://public-files.dreamness.cn/");

    private static readonly AssetCenterTrustedKey TrustedKey = new(
        "ed25519-sha256:QnOwhaW3do0UCg3Ri-x4sHwCGtGcgLIweQtxiiOFEEY",
        "uW-ErOGm8tB5JrRopLgkr5emv_byQcuc94uCvlo2P2Y");

    private readonly object _gate = new();
    private AssetCenterClient? _client;
    private AssetCenterUpdateCheckResult? _lastCheck;
    private AssetCenterStagedArtifact? _staged;

    public AssetCenterAppUpdateService()
    {
        CurrentVersion = ResolveCurrentVersion();
        Channel = StableChannel;
        AssetId = CompanionAssetId;
        InstallRoot = Environment.GetEnvironmentVariable("ASSETCENTER_INSTALL_ROOT");
        IsInstallLayoutAvailable = !string.IsNullOrWhiteSpace(InstallRoot)
            && System.IO.Directory.Exists(InstallRoot);
    }

    public string CurrentVersion { get; }

    public string Channel { get; }

    public string AssetId { get; }

    public bool IsInstallLayoutAvailable { get; }

    private string? InstallRoot { get; }

    public async Task ConfirmHealthyStartIfNeededAsync(CancellationToken cancellationToken = default)
    {
        var token = Environment.GetEnvironmentVariable("ASSETCENTER_HEALTH_TOKEN");
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(InstallRoot))
        {
            return;
        }

        var layout = new InstallationLayout(InstallRoot);
        var activation = new ActivationCoordinator(layout, new InstallationStateStore(layout));
        await activation.ConfirmApplicationHealthAsync(token, cancellationToken);
    }

    public async Task<AppUpdateCheckOutcome> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetOrCreateClient();
            var check = await client.CheckForUpdateAsync(cancellationToken);
            lock (_gate)
            {
                _lastCheck = check;
                _staged = null;
            }

            return check.Status switch
            {
                AssetCenterUpdateStatus.Available when IsSameOrOlder(check.Release?.Version, CurrentVersion) =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.UpToDate,
                        check.Release?.Version,
                        "Already on the channel release."),
                AssetCenterUpdateStatus.Available =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.Available,
                        check.Release?.Version,
                        "Update available."),
                AssetCenterUpdateStatus.NotEligible =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.NotEligible,
                        check.Release?.Version,
                        "Not eligible for the current rollout."),
                AssetCenterUpdateStatus.NotApplicable =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.NotApplicable,
                        check.Release?.Version,
                        "No matching artifact for this platform."),
                AssetCenterUpdateStatus.Withdrawn =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.Withdrawn,
                        null,
                        "Channel publication was withdrawn."),
                _ =>
                    new AppUpdateCheckOutcome(
                        AppUpdateAvailability.Failed,
                        null,
                        "Unexpected update status."),
            };
        }
        catch (Exception ex)
        {
            var code = (ex as AssetCenterException)?.Code;
            return new AppUpdateCheckOutcome(
                AppUpdateAvailability.Failed,
                null,
                ex.Message,
                code);
        }
    }

    public async Task<AppUpdateStageOutcome> DownloadAndStageAsync(
        IProgress<AppUpdateProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        AssetCenterUpdateCheckResult? check;
        lock (_gate)
        {
            check = _lastCheck;
        }

        if (check is null || check.Status != AssetCenterUpdateStatus.Available || check.Release is null)
        {
            var refreshed = await CheckForUpdateAsync(cancellationToken);
            if (refreshed.Availability != AppUpdateAvailability.Available)
            {
                return new AppUpdateStageOutcome(
                    false,
                    refreshed.AvailableVersion,
                    null,
                    refreshed.Message,
                    refreshed.ErrorCode);
            }

            lock (_gate)
            {
                check = _lastCheck;
            }
        }

        if (check is null || check.Status != AssetCenterUpdateStatus.Available)
        {
            return new AppUpdateStageOutcome(false, null, null, "No update available to download.");
        }

        try
        {
            var client = GetOrCreateClient();
            IProgress<AssetCenterProgress>? sdkProgress = progress is null
                ? null
                : new Progress<AssetCenterProgress>(p =>
                    progress.Report(new AppUpdateProgress(p.Stage.ToString(), p.CompletedBytes, p.TotalBytes)));

            var downloaded = sdkProgress is null
                ? await client.DownloadArtifactAsync(check, cancellationToken)
                : await client.DownloadArtifactAsync(check, sdkProgress, cancellationToken);
            var staged = sdkProgress is null
                ? await client.StageArtifactAsync(check, downloaded, cancellationToken)
                : await client.StageArtifactAsync(check, downloaded, sdkProgress, cancellationToken);

            lock (_gate)
            {
                _staged = staged;
            }

            return new AppUpdateStageOutcome(
                true,
                staged.Version,
                staged.Path,
                "Update staged successfully.");
        }
        catch (Exception ex)
        {
            var code = (ex as AssetCenterException)?.Code;
            return new AppUpdateStageOutcome(false, check.Release?.Version, null, ex.Message, code);
        }
    }

    public async Task<AppUpdateApplyOutcome> ApplyStagedAndExitAsync(CancellationToken cancellationToken = default)
    {
        AssetCenterStagedArtifact? staged;
        AssetCenterUpdateCheckResult? check;
        lock (_gate)
        {
            staged = _staged;
            check = _lastCheck;
        }

        if (staged is null)
        {
            return new AppUpdateApplyOutcome(false, false, "No staged update to apply.");
        }

        if (!IsInstallLayoutAvailable || string.IsNullOrWhiteSpace(InstallRoot))
        {
            return new AppUpdateApplyOutcome(
                false,
                false,
                "Install layout not available. Staged at: " + staged.Path
                + ". Launch via AssetCenter Bootstrapper to apply.");
        }

        try
        {
            var entryPoint = staged.EntryPoints.FirstOrDefault(static e =>
                    e.Equals("Ra3MapUtils.exe", StringComparison.OrdinalIgnoreCase))
                ?? staged.EntryPoints.FirstOrDefault()
                ?? "Ra3MapUtils.exe";

            var layout = new InstallationLayout(InstallRoot);
            var activation = new ActivationCoordinator(layout, new InstallationStateStore(layout));
            await activation.PrepareApplicationAsync(
                staged.Path,
                check?.Release?.ReleaseId ?? staged.ReleaseId,
                staged.Version,
                entryPoint,
                cancellationToken);

            // Exit so Bootstrapper can activate the pending version on next launch.
            _ = Task.Run(async () =>
            {
                await Task.Delay(400);
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    System.Windows.Application.Current.Shutdown());
            });

            return new AppUpdateApplyOutcome(
                true,
                true,
                "Update prepared. The application will exit; relaunch from Bootstrapper to activate.");
        }
        catch (Exception ex)
        {
            return new AppUpdateApplyOutcome(false, false, ex.Message);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _client?.Dispose();
            _client = null;
        }
    }

    private AssetCenterClient GetOrCreateClient()
    {
        lock (_gate)
        {
            if (_client is not null)
            {
                return _client;
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var stateDirectory = System.IO.Path.Combine(localAppData, "Ra3MapUtils", "updates", "app");
            System.IO.Directory.CreateDirectory(stateDirectory);

            _client = new AssetCenterClient(new AssetCenterClientOptions
            {
                AssetId = CompanionAssetId,
                Channel = StableChannel,
                DistributionEndpoint = DistributionEndpoint,
                TrustedKeys = [TrustedKey],
                Target = AssetCenterTarget.DetectCurrent(UpdaterProtocolVersion),
                UpdatePolicy = AssetCenterUpdatePolicy.Prompt,
                StateDirectory = stateDirectory,
                StagingDirectory = System.IO.Path.Combine(stateDirectory, "staging"),
                TelemetryPreference = AssetCenterTelemetryPreference.Disabled,
                TelemetryEndpoint = null,
            });
            return _client;
        }
    }

    private static string ResolveCurrentVersion()
    {
        var informational = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus >= 0 ? informational[..plus] : informational.Trim();
        }

        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static bool IsSameOrOlder(string? channelVersion, string currentVersion)
    {
        if (string.IsNullOrWhiteSpace(channelVersion))
        {
            return false;
        }

        if (string.Equals(Normalize(channelVersion), Normalize(currentVersion), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (TryParseSemVer(channelVersion, out var remote)
            && TryParseSemVer(currentVersion, out var local))
        {
            return Compare(remote, local) <= 0;
        }

        return false;
    }

    private static string Normalize(string version) => version.Trim().TrimStart('v', 'V');

    private static bool TryParseSemVer(string text, out (int Major, int Minor, int Patch) version)
    {
        version = default;
        var core = Normalize(text);
        var dash = core.IndexOf('-');
        if (dash >= 0)
        {
            core = core[..dash];
        }

        var parts = core.Split('.');
        if (parts.Length < 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
        {
            return false;
        }

        var patch = 0;
        if (parts.Length >= 3
            && !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out patch))
        {
            return false;
        }

        version = (major, minor, patch);
        return true;
    }

    private static int Compare((int Major, int Minor, int Patch) left, (int Major, int Minor, int Patch) right)
    {
        var major = left.Major.CompareTo(right.Major);
        if (major != 0)
        {
            return major;
        }

        var minor = left.Minor.CompareTo(right.Minor);
        return minor != 0 ? minor : left.Patch.CompareTo(right.Patch);
    }
}
