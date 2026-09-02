using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Ra3Hacker.Sdk;
using UI.Services;

namespace UI.ViewModels.Tools;

public partial class LuaExecutorWindowViewModel : ObservableObject
{
    private const string MapLuaState = "isolated";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private readonly ILocalizationService _localization;

    public LuaExecutorWindowViewModel(ILocalizationService localization)
    {
        _localization = localization;
        RefreshLocalized();
        StatusText = _localization.GetString(
            "LuaExec_NeedDebugger",
            "当前环境：地图 Lua。请先打开调试工具，然后执行 Lua。");
        _localization.LanguageChanged += (_, _) => RefreshLocalized();
    }

    [ObservableProperty] private bool _isTopmost;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _windowTitle = "";
    [ObservableProperty] private string _luaCode = "print(GetFrame())";
    [ObservableProperty] private string _luaFilePath = "";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _returnsText = "";
    [ObservableProperty] private string _outputText = "";
    [ObservableProperty] private string _responseText = "";
    [ObservableProperty] private string _runCodeLabel = "";
    [ObservableProperty] private string _runFileLabel = "";
    [ObservableProperty] private string _browseFileLabel = "";
    [ObservableProperty] private string _codeLabel = "";
    [ObservableProperty] private string _fileLabel = "";
    [ObservableProperty] private string _returnsLabel = "";
    [ObservableProperty] private string _outputLabel = "";

    [RelayCommand]
    private void BrowseLuaFile()
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Lua (*.lua)|*.lua|All (*.*)|*.*",
            Title = _localization.GetString("LuaExec_BrowseTitle", "选择 Lua 文件"),
        };

        if (!string.IsNullOrWhiteSpace(LuaFilePath))
        {
            var dir = Path.GetDirectoryName(LuaFilePath);
            if (Directory.Exists(dir))
            {
                dialog.InitialDirectory = dir;
            }
        }

        if (dialog.ShowDialog() == true)
        {
            LuaFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task RunCode()
    {
        if (string.IsNullOrWhiteSpace(LuaCode))
        {
            StatusText = _localization.GetString("LuaExec_EmptyCode", "Lua 代码不能为空。");
            return;
        }

        await RunLuaCommandAsync(
            _localization.GetString("LuaExec_RunCodeAction", "执行代码"),
            (client, ct) => client.Lua.ExecuteAsync(LuaCode, MapLuaState, ct));
    }

    [RelayCommand]
    private async Task RunFile()
    {
        if (string.IsNullOrWhiteSpace(LuaFilePath) || !File.Exists(LuaFilePath))
        {
            StatusText = _localization.GetString("LuaExec_InvalidFile", "请选择有效的 Lua 文件。");
            return;
        }

        var fullPath = Path.GetFullPath(LuaFilePath);
        await RunLuaCommandAsync(
            _localization.GetString("LuaExec_RunFileAction", "执行文件"),
            (client, ct) => client.Lua.ExecuteFileAsync(fullPath, MapLuaState, ct));
    }

    private async Task RunLuaCommandAsync(
        string actionName,
        Func<Ra3HackerClient, CancellationToken, Task<LuaExecuteResponse>> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = string.Format(_localization.GetString("LuaExec_Running", "{0}中..."), actionName);
        ReturnsText = "";
        OutputText = "";
        ResponseText = "";

        try
        {
            using var client = CreateClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var response = await action(client, timeout.Token);
            ApplyLuaResponse(actionName, response);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(
                _localization.GetString(
                    "LuaExec_ApiDown",
                    "{0}失败：无法连接调试器 API，请先打开调试工具，并确认 RA3 已启动。"),
                actionName);
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

    private void ApplyLuaResponse(string actionName, LuaExecuteResponse response)
    {
        ReturnsText = FormatLines(response.Returns, _localization.GetString("LuaExec_NoReturns", "(无返回值)"));
        OutputText = FormatLines(response.Output, _localization.GetString("LuaExec_NoOutput", "(无输出)"));
        ResponseText = JsonSerializer.Serialize(response, JsonOptions);

        var resultText = response.Ok
            ? _localization.GetString("LuaExec_Ok", "完成")
            : _localization.GetString("LuaExec_Fail", "失败");
        StatusText = $"{actionName}{resultText}：ResultCode={response.ResultCode}";
        if (!response.Ok && !string.IsNullOrWhiteSpace(response.Message))
        {
            StatusText += $"，{response.Message}";
        }
    }

    private static string FormatLines(IEnumerable<string>? lines, string empty)
    {
        if (lines is null)
        {
            return empty;
        }

        var list = lines.Where(l => l is not null).ToList();
        return list.Count == 0 ? empty : string.Join(Environment.NewLine, list);
    }

    private void RefreshLocalized()
    {
        WindowTitle = _localization.GetString("LuaExec_Title", "Lua 执行器");
        RunCodeLabel = _localization.GetString("LuaExec_RunCode", "执行代码");
        RunFileLabel = _localization.GetString("LuaExec_RunFile", "执行文件");
        BrowseFileLabel = _localization.GetString("LuaExec_Browse", "浏览…");
        CodeLabel = _localization.GetString("LuaExec_Code", "Lua 代码");
        FileLabel = _localization.GetString("LuaExec_File", "Lua 文件");
        ReturnsLabel = _localization.GetString("LuaExec_Returns", "返回值");
        OutputLabel = _localization.GetString("LuaExec_Output", "输出");
    }
}
