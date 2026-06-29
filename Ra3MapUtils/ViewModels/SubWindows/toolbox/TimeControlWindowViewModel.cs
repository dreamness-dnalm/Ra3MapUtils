using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3Hacker.Sdk;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class TimeControlWindowViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private TimeControlStatus? _lastTimeStatus;

    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private bool _isBusy = false;

    [ObservableProperty] private string _statusText = "请先打开调试工具，然后刷新时间状态。";

    [ObservableProperty] private string _currentFrameText = "-";

    [ObservableProperty] private string _currentTimeText = "--:--:--.--";

    [ObservableProperty] private string _pauseStatusText = "-";

    [ObservableProperty] private string _fastStatusText = "-";

    [ObservableProperty] private string _singleStepStatusText = "-";

    [ObservableProperty] private string _pauseButtonText = "暂停";

    [ObservableProperty] private string _fastButtonText = "快速运行";

    [ObservableProperty] private string _singleStepButtonText = "单步模式";

    [ObservableProperty] private string _fastToFrameText = "900";

    [ObservableProperty] private string _fastToHourText = "0";

    [ObservableProperty] private string _fastToMinuteText = "1";

    [ObservableProperty] private string _fastToSecondText = "0";

    [ObservableProperty] private string _fastToSubFrameText = "0";

    [ObservableProperty] private string _outputText = "";

    [RelayCommand]
    private async Task RefreshStatus()
    {
        await RunTimeCommandAsync(client => client.Time.GetAsync());
    }

    [RelayCommand]
    private async Task TogglePause()
    {
        await RunTimeCommandAsync(client => client.Time.SetPauseAsync(!(_lastTimeStatus?.Paused ?? false)));
    }

    [RelayCommand]
    private async Task ToggleFast()
    {
        await RunTimeCommandAsync(client => client.Time.SetFastAsync(!(_lastTimeStatus?.RunFast ?? false)));
    }

    [RelayCommand]
    private async Task ToggleSingleStep()
    {
        await RunTimeCommandAsync(client => client.Time.SetSingleStepAsync(!(_lastTimeStatus?.SingleStep ?? false)));
    }

    [RelayCommand]
    private async Task StepFrame()
    {
        await RunTimeCommandAsync(client => client.Time.StepAsync());
    }

    [RelayCommand]
    private async Task ResetTimeControl()
    {
        await RunTimeCommandAsync(client => client.Time.ResetAsync());
    }

    [RelayCommand]
    private async Task FastToFrame()
    {
        if (!uint.TryParse(FastToFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frame))
        {
            ShowValidationError("帧序号必须是非负整数。");
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
            ShowValidationError("时间格式不正确：分钟/秒为 0-59，子帧为 0-14。");
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
        StatusText = "时间命令执行中...";

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
            StatusText = "时间命令失败：无法连接 Ra3Hacker API，请先在工具箱点击“打开调试工具”，并确认 RA3 已启动。";
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
        PauseStatusText = status.Paused ? "暂停中" : "运行中";
        FastStatusText = status.RunFast ? "快速运行" : "正常速度";
        SingleStepStatusText = status.SingleStep ? "单步模式" : "关闭";

        PauseButtonText = status.Paused ? "继续" : "暂停";
        FastButtonText = status.RunFast ? "停止快速" : "快速运行";
        SingleStepButtonText = status.SingleStep ? "退出单步" : "单步模式";
        StatusText = $"当前帧：{CurrentFrameText}，时间：{CurrentTimeText}";
    }

    private void ShowValidationError(string message)
    {
        StatusText = message;
        OutputText = message;
    }

    private static bool TryParseInt(string text, int min, int max, out int value)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
               value >= min &&
               value <= max;
    }
}
