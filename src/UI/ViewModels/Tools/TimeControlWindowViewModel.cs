using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3Hacker.Sdk;
using UI.Services;

namespace UI.ViewModels.Tools;

public partial class TimeControlWindowViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private readonly ILocalizationService _localization;
    private TimeControlStatus? _lastTimeStatus;

    public TimeControlWindowViewModel(ILocalizationService localization)
    {
        _localization = localization;
        RefreshLocalized();
        StatusText = _localization.GetString("TimeCtrl_NeedDebugger", "请先打开调试工具，然后刷新时间状态。");
        _localization.LanguageChanged += (_, _) =>
        {
            RefreshLocalized();
            if (_lastTimeStatus is not null)
            {
                ApplyTimeStatus(_lastTimeStatus);
            }
        };
    }

    [ObservableProperty] private bool _isTopmost;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _windowTitle = "";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _currentFrameText = "-";
    [ObservableProperty] private string _currentTimeText = "--:--:--.--";
    [ObservableProperty] private string _pauseStatusText = "-";
    [ObservableProperty] private string _fastStatusText = "-";
    [ObservableProperty] private string _singleStepStatusText = "-";
    [ObservableProperty] private string _pauseButtonText = "";
    [ObservableProperty] private string _fastButtonText = "";
    [ObservableProperty] private string _singleStepButtonText = "";
    [ObservableProperty] private string _fastToFrameText = "900";
    [ObservableProperty] private string _fastToHourText = "0";
    [ObservableProperty] private string _fastToMinuteText = "1";
    [ObservableProperty] private string _fastToSecondText = "0";
    [ObservableProperty] private string _fastToSubFrameText = "0";
    [ObservableProperty] private string _outputText = "";
    [ObservableProperty] private string _refreshLabel = "";
    [ObservableProperty] private string _stepLabel = "";
    [ObservableProperty] private string _resetLabel = "";
    [ObservableProperty] private string _fastToFrameLabel = "";
    [ObservableProperty] private string _fastToTimeLabel = "";
    [ObservableProperty] private string _runLabel = "";

    [RelayCommand]
    private async Task RefreshStatus() => await RunTimeCommandAsync(client => client.Time.GetAsync());

    [RelayCommand]
    private async Task TogglePause() =>
        await RunTimeCommandAsync(client => client.Time.SetPauseAsync(!(_lastTimeStatus?.Paused ?? false)));

    [RelayCommand]
    private async Task ToggleFast() =>
        await RunTimeCommandAsync(client => client.Time.SetFastAsync(!(_lastTimeStatus?.RunFast ?? false)));

    [RelayCommand]
    private async Task ToggleSingleStep() =>
        await RunTimeCommandAsync(client => client.Time.SetSingleStepAsync(!(_lastTimeStatus?.SingleStep ?? false)));

    [RelayCommand]
    private async Task StepFrame() => await RunTimeCommandAsync(client => client.Time.StepAsync());

    [RelayCommand]
    private async Task ResetTimeControl() => await RunTimeCommandAsync(client => client.Time.ResetAsync());

    [RelayCommand]
    private async Task FastToFrame()
    {
        if (!uint.TryParse(FastToFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frame))
        {
            StatusText = _localization.GetString("TimeCtrl_InvalidFrame", "帧序号必须是非负整数。");
            return;
        }

        await RunTimeCommandAsync(client => client.Time.FastToFrameAsync(frame));
    }

    [RelayCommand]
    private async Task FastToTime()
    {
        if (!TryParseInt(FastToHourText, 0, int.MaxValue, out var hour) ||
            !TryParseInt(FastToMinuteText, 0, 59, out var minute) ||
            !TryParseInt(FastToSecondText, 0, 59, out var second) ||
            !TryParseInt(FastToSubFrameText, 0, 14, out var subframe))
        {
            StatusText = _localization.GetString("TimeCtrl_InvalidTime", "时间格式不正确：分钟/秒为 0-59，子帧为 0-14。");
            return;
        }

        await RunTimeCommandAsync(client => client.Time.FastToTimeAsync(hour, minute, second, subframe));
    }

    private async Task RunTimeCommandAsync(Func<Ra3HackerClient, Task<TimeControlStatus>> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = _localization.GetString("TimeCtrl_Running", "时间命令执行中...");

        try
        {
            using var client = CreateClient();
            var status = await action(client);
            _lastTimeStatus = status;
            ApplyTimeStatus(status);
            OutputText = JsonSerializer.Serialize(status, JsonOptions);
        }
        catch (Exception ex)
        {
            StatusText = _localization.GetString(
                "TimeCtrl_ApiDown",
                "时间命令失败：无法连接调试器 API，请先打开调试工具，并确认 RA3 已启动。");
            OutputText = ex.ToString();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Ra3HackerClient CreateClient()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "data", "Ra3Hacker", "setting.json");
        return new Ra3HackerClient(Ra3HackerOptions.FromSettingsFile(settingsPath));
    }

    private void ApplyTimeStatus(TimeControlStatus status)
    {
        CurrentFrameText = status.CurrentFrame?.ToString(CultureInfo.InvariantCulture) ?? "-";
        CurrentTimeText = status.CurrentTimeText;
        PauseStatusText = status.Paused
            ? _localization.GetString("TimeCtrl_Paused", "暂停中")
            : _localization.GetString("TimeCtrl_RunningState", "运行中");
        FastStatusText = status.RunFast
            ? _localization.GetString("TimeCtrl_FastOn", "快速运行")
            : _localization.GetString("TimeCtrl_FastOff", "正常速度");
        SingleStepStatusText = status.SingleStep
            ? _localization.GetString("TimeCtrl_StepOn", "单步模式")
            : _localization.GetString("TimeCtrl_StepOff", "关闭");

        PauseButtonText = status.Paused
            ? _localization.GetString("TimeCtrl_Resume", "继续")
            : _localization.GetString("TimeCtrl_Pause", "暂停");
        FastButtonText = status.RunFast
            ? _localization.GetString("TimeCtrl_StopFast", "停止快速")
            : _localization.GetString("TimeCtrl_StartFast", "快速运行");
        SingleStepButtonText = status.SingleStep
            ? _localization.GetString("TimeCtrl_ExitStep", "退出单步")
            : _localization.GetString("TimeCtrl_EnterStep", "单步模式");
        StatusText = string.Format(
            _localization.GetString("TimeCtrl_StatusFormat", "当前帧：{0}，时间：{1}"),
            CurrentFrameText,
            CurrentTimeText);
    }

    private void RefreshLocalized()
    {
        WindowTitle = _localization.GetString("TimeCtrl_Title", "时间操控");
        RefreshLabel = _localization.GetString("TimeCtrl_Refresh", "刷新状态");
        StepLabel = _localization.GetString("TimeCtrl_StepFrame", "前进一帧");
        ResetLabel = _localization.GetString("TimeCtrl_Reset", "重置");
        FastToFrameLabel = _localization.GetString("TimeCtrl_FastToFrame", "快进到帧");
        FastToTimeLabel = _localization.GetString("TimeCtrl_FastToTime", "快进到时间");
        RunLabel = _localization.GetString("TimeCtrl_Run", "执行");
        if (_lastTimeStatus is null)
        {
            PauseButtonText = _localization.GetString("TimeCtrl_Pause", "暂停");
            FastButtonText = _localization.GetString("TimeCtrl_StartFast", "快速运行");
            SingleStepButtonText = _localization.GetString("TimeCtrl_EnterStep", "单步模式");
        }
    }

    private static bool TryParseInt(string text, int min, int max, out int value) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
        value >= min &&
        value <= max;
}
