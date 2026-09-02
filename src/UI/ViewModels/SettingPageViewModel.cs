using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.LuaImport;
using Core.Settings;
using Core.Updates;
using Ra3MapUtils.Utils;
using UI.Services;

namespace UI.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    private readonly ILocalizationService _localization;
    private readonly ILuaImportSettingsStore _luaImportSettings;
    private readonly IAppUpdateService _appUpdate;
    private bool _suppressPersist;
    private bool _hasStagedUpdate;

    public SettingPageViewModel(
        ILocalizationService localization,
        ILuaImportSettingsStore luaImportSettings,
        IAppUpdateService appUpdate)
    {
        _localization = localization;
        _luaImportSettings = luaImportSettings;
        _appUpdate = appUpdate;
        LanguageOptions =
        [
            new LanguageOption(UiLanguageCodes.ZhCn, localization.GetString("Settings_Language_Zh", "简体中文")),
            new LanguageOption(UiLanguageCodes.En, localization.GetString("Settings_Language_En", "English")),
        ];

        _suppressPersist = true;
        SelectedLanguageCode = localization.CurrentLanguage;
        RedundancyFactorText = _luaImportSettings.GetRedundancyFactor().ToString();
        CurrentVersionText = _appUpdate.CurrentVersion;
        _suppressPersist = false;

        RefreshLabels();
        UpdateStatusText = _localization.GetString("Settings_UpdateIdle", "尚未检查更新。");
        _localization.LanguageChanged += (_, _) =>
        {
            RefreshLabels();
            RefreshLanguageOptionLabels();
        };
    }

    public IReadOnlyList<LanguageOption> LanguageOptions { get; }

    [ObservableProperty] private string _pageTitle = "设置";
    [ObservableProperty] private string _pageSubtitle = "应用偏好";
    [ObservableProperty] private string _languageLabel = "语言";
    [ObservableProperty] private string _languageHint = "";
    [ObservableProperty] private string _selectedLanguageCode = UiLanguageCodes.ZhCn;
    [ObservableProperty] private string _luaSectionTitle = "Lua 导入";
    [ObservableProperty] private string _redundancyLabel = "冗余因子";
    [ObservableProperty] private string _redundancyHint = "";
    [ObservableProperty] private string _redundancyApplyLabel = "应用";
    [ObservableProperty] private string _redundancyFactorText = "100";
    [ObservableProperty] private string _redundancyStatus = "";

    [ObservableProperty] private string _updateSectionTitle = "";
    [ObservableProperty] private string _updateChannelHint = "";
    [ObservableProperty] private string _updateVersionLabel = "";
    [ObservableProperty] private string _currentVersionText = "";
    [ObservableProperty] private string _checkUpdateLabel = "";
    [ObservableProperty] private string _downloadUpdateLabel = "";
    [ObservableProperty] private string _applyUpdateLabel = "";
    [ObservableProperty] private string _updateStatusText = "";
    [ObservableProperty] private bool _isUpdateBusy;
    [ObservableProperty] private bool _canDownloadUpdate;
    [ObservableProperty] private bool _canApplyUpdate;
    [ObservableProperty] private double _updateProgressRatio;
    [ObservableProperty] private bool _isUpdateProgressVisible;

    partial void OnSelectedLanguageCodeChanged(string value)
    {
        if (_suppressPersist || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (string.Equals(value, _localization.CurrentLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _localization.ApplyLanguage(value, persist: true);
    }

    [RelayCommand]
    private void ApplyRedundancyFactor()
    {
        if (!int.TryParse(RedundancyFactorText.Trim(), out var factor) || factor < 0)
        {
            RedundancyStatus = _localization.GetString(
                "Settings_LuaRedundancyInvalid",
                "请输入非负整数。");
            return;
        }

        _luaImportSettings.SetRedundancyFactor(factor);
        MapLuaImporterUtil.InvalidateRedundancyCache();
        RedundancyFactorText = factor.ToString();
        RedundancyStatus = _localization.GetString(
            "Settings_LuaRedundancySaved",
            "已保存，下次导入生效。");
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        if (IsUpdateBusy)
        {
            return;
        }

        IsUpdateBusy = true;
        CanDownloadUpdate = false;
        CanApplyUpdate = false;
        _hasStagedUpdate = false;
        IsUpdateProgressVisible = false;
        UpdateStatusText = _localization.GetString("Settings_UpdateChecking", "正在检查更新…");

        try
        {
            var result = await _appUpdate.CheckForUpdateAsync();
            UpdateStatusText = result.Availability switch
            {
                AppUpdateAvailability.Available => string.Format(
                    _localization.GetString("Settings_UpdateAvailable", "发现新版本：{0}"),
                    result.AvailableVersion ?? "?"),
                AppUpdateAvailability.UpToDate => _localization.GetString(
                    "Settings_UpdateUpToDate",
                    "已是最新版本。"),
                AppUpdateAvailability.NotEligible => _localization.GetString(
                    "Settings_UpdateNotEligible",
                    "当前不在更新灰度范围内。"),
                AppUpdateAvailability.NotApplicable => _localization.GetString(
                    "Settings_UpdateNotApplicable",
                    "没有适用于本机的更新包。"),
                AppUpdateAvailability.Withdrawn => _localization.GetString(
                    "Settings_UpdateWithdrawn",
                    "更新通道已撤回。"),
                _ => string.Format(
                    _localization.GetString("Settings_UpdateCheckFailed", "检查更新失败：{0}"),
                    result.Message),
            };
            CanDownloadUpdate = result.Availability == AppUpdateAvailability.Available;
        }
        finally
        {
            IsUpdateBusy = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAndStageAsync()
    {
        if (IsUpdateBusy || !CanDownloadUpdate)
        {
            return;
        }

        IsUpdateBusy = true;
        CanApplyUpdate = false;
        _hasStagedUpdate = false;
        IsUpdateProgressVisible = true;
        UpdateProgressRatio = 0;
        UpdateStatusText = _localization.GetString("Settings_UpdateDownloading", "正在下载并校验…");

        try
        {
            var progress = new Progress<AppUpdateProgress>(p =>
            {
                UpdateProgressRatio = p.TotalBytes <= 0
                    ? 0
                    : (double)p.CompletedBytes / p.TotalBytes;
            });

            var result = await _appUpdate.DownloadAndStageAsync(progress);
            if (result.Success)
            {
                _hasStagedUpdate = true;
                CanApplyUpdate = true;
                UpdateStatusText = string.Format(
                    _localization.GetString(
                        "Settings_UpdateStaged",
                        "已暂存版本 {0}。请应用并退出，然后通过 Bootstrapper 启动以激活。"),
                    result.StagedVersion ?? "?");
                if (!_appUpdate.IsInstallLayoutAvailable)
                {
                    UpdateStatusText = string.Format(
                        _localization.GetString(
                            "Settings_UpdateStagedDev",
                            "已暂存版本 {0}（开发布局无法自动激活）。路径：{1}"),
                        result.StagedVersion ?? "?",
                        result.StagedPath ?? "");
                    CanApplyUpdate = false;
                }
            }
            else
            {
                UpdateStatusText = string.Format(
                    _localization.GetString("Settings_UpdateDownloadFailed", "下载/校验失败：{0}"),
                    result.Message);
            }
        }
        finally
        {
            IsUpdateBusy = false;
            IsUpdateProgressVisible = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAndExitAsync()
    {
        if (IsUpdateBusy || !_hasStagedUpdate)
        {
            return;
        }

        IsUpdateBusy = true;
        UpdateStatusText = _localization.GetString("Settings_UpdateApplying", "正在准备激活并退出…");
        try
        {
            var result = await _appUpdate.ApplyStagedAndExitAsync();
            UpdateStatusText = result.Success
                ? _localization.GetString("Settings_UpdateApplyScheduled", "已安排退出以激活更新。")
                : string.Format(
                    _localization.GetString("Settings_UpdateApplyFailed", "无法应用更新：{0}"),
                    result.Message);
        }
        finally
        {
            IsUpdateBusy = false;
        }
    }

    private void RefreshLabels()
    {
        PageTitle = _localization.GetString("Settings_Title", "设置");
        PageSubtitle = _localization.GetString("Settings_Subtitle", "应用偏好");
        LanguageLabel = _localization.GetString("Settings_Language", "语言");
        LanguageHint = _localization.GetString("Settings_LanguageHint");
        LuaSectionTitle = _localization.GetString("Settings_LuaSection", "Lua 导入");
        RedundancyLabel = _localization.GetString("Settings_LuaRedundancy", "冗余因子");
        RedundancyHint = _localization.GetString(
            "Settings_LuaRedundancyHint",
            "导入时在脚本末尾追加的填充行数，用于缓解截断。默认 100。");
        RedundancyApplyLabel = _localization.GetString("Settings_LuaRedundancyApply", "应用");

        UpdateSectionTitle = _localization.GetString("Settings_UpdateSection", "应用更新");
        UpdateChannelHint = string.Format(
            _localization.GetString(
                "Settings_UpdateChannelHint",
                "通过 AssetCenter 检查稳定通道（{0}）更新。当前频道：{1}"),
            _appUpdate.AssetId,
            _appUpdate.Channel);
        UpdateVersionLabel = _localization.GetString("Settings_UpdateCurrentVersion", "当前版本：");
        CheckUpdateLabel = _localization.GetString("Settings_UpdateCheck", "检查更新");
        DownloadUpdateLabel = _localization.GetString("Settings_UpdateDownload", "下载并暂存");
        ApplyUpdateLabel = _localization.GetString("Settings_UpdateApply", "应用并退出");
    }

    private void RefreshLanguageOptionLabels()
    {
        foreach (var option in LanguageOptions)
        {
            option.DisplayName = option.Code == UiLanguageCodes.ZhCn
                ? _localization.GetString("Settings_Language_Zh", "简体中文")
                : _localization.GetString("Settings_Language_En", "English");
        }
    }
}

public partial class LanguageOption : ObservableObject
{
    public LanguageOption(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public string Code { get; }

    [ObservableProperty]
    private string _displayName;
}
