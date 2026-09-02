namespace Core.Updates;

/// <summary>
/// Companion application update facade (asset <c>cn.dreamness.ra3maputils</c>).
/// Parallel content assets (e.g. Lua library) MUST use a separate service + AssetId + state directory.
/// </summary>
public interface IAppUpdateService
{
    string CurrentVersion { get; }

    string Channel { get; }

    string AssetId { get; }

    bool IsInstallLayoutAvailable { get; }

    Task ConfirmHealthyStartIfNeededAsync(CancellationToken cancellationToken = default);

    Task<AppUpdateCheckOutcome> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    Task<AppUpdateStageOutcome> DownloadAndStageAsync(
        IProgress<AppUpdateProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares activation when an install root is present, then requests application exit.
    /// </summary>
    Task<AppUpdateApplyOutcome> ApplyStagedAndExitAsync(CancellationToken cancellationToken = default);
}

public enum AppUpdateAvailability
{
    UpToDate,
    Available,
    NotEligible,
    NotApplicable,
    Withdrawn,
    Failed,
}

public sealed record AppUpdateCheckOutcome(
    AppUpdateAvailability Availability,
    string? AvailableVersion,
    string Message,
    string? ErrorCode = null);

public sealed record AppUpdateProgress(string Stage, long CompletedBytes, long TotalBytes);

public sealed record AppUpdateStageOutcome(
    bool Success,
    string? StagedVersion,
    string? StagedPath,
    string Message,
    string? ErrorCode = null);

public sealed record AppUpdateApplyOutcome(
    bool Success,
    bool ExitScheduled,
    string Message,
    string? ErrorCode = null);
