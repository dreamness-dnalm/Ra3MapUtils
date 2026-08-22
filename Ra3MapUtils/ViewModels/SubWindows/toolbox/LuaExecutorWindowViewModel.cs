using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3Hacker.Sdk;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class LuaExecutorWindowViewModel : ObservableObject, IDisposable
{
    private const string MapLuaState = "isolated";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private readonly ILuaCompletionService _luaCompletionService;
    private bool _completionOptionsLoaded;

    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private bool _isBusy = false;

    [ObservableProperty] private string _luaCode = "print(GetFrame())";

    [ObservableProperty] private string _luaFilePath = "";

    [ObservableProperty] private string _statusText = "当前环境：地图lua。请先打开调试工具，然后执行 Lua。";

    [ObservableProperty] private string _returnsText = "";

    [ObservableProperty] private string _outputText = "";

    [ObservableProperty] private string _responseText = "";

    [ObservableProperty] private bool _enableLuaLibrary = true;

    [ObservableProperty] private string _userCodeLibraryPath = "";

    [ObservableProperty] private string _completionStatusText = "Lua 4 补全索引尚未加载";

    public LuaExecutorWindowViewModel(ILuaCompletionService luaCompletionService)
    {
        _luaCompletionService = luaCompletionService;
        var options = _luaCompletionService.GetOptions();
        _enableLuaLibrary = options.EnableLuaLibrary;
        _userCodeLibraryPath = options.UserCodeLibraryPath;
        CompletionStatusText = _luaCompletionService.Status.Message;
        _completionOptionsLoaded = true;
        _luaCompletionService.IndexChanged += OnCompletionIndexChanged;
    }

    public async Task InitializeCompletionAsync()
    {
        await _luaCompletionService.RefreshAsync();
    }

    public IReadOnlyList<LuaCompletionItem> GetCompletionItems(string code, int caretOffset)
    {
        return _luaCompletionService.GetCompletions(code, caretOffset);
    }

    public LuaCompletionItem? GetDocumentation(string code, int offset)
    {
        return _luaCompletionService.GetDocumentation(code, offset);
    }

    partial void OnEnableLuaLibraryChanged(bool value)
    {
        if (_completionOptionsLoaded)
        {
            _ = SaveCompletionOptionsAsync();
        }
    }

    partial void OnUserCodeLibraryPathChanged(string value)
    {
        if (_completionOptionsLoaded)
        {
            _ = SaveCompletionOptionsAsync();
        }
    }

    [RelayCommand]
    private void BrowseLuaFile()
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Lua 文件 (*.lua)|*.lua|所有文件 (*.*)|*.*",
            Title = "选择 Lua 文件",
        };

        TryApplyDialogDirectory(dialog, LuaFilePath);

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        LuaFilePath = dialog.FileName;
    }

    [RelayCommand]
    private void BrowseUserCodeLibrary()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择用户 Lua 4 代码库根目录",
            FolderName = Directory.Exists(UserCodeLibraryPath) ? UserCodeLibraryPath : "",
        };

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
        {
            UserCodeLibraryPath = dialog.FolderName;
        }
    }

    [RelayCommand]
    private void ClearUserCodeLibrary()
    {
        UserCodeLibraryPath = "";
    }

    [RelayCommand]
    private async Task RefreshCompletion()
    {
        await _luaCompletionService.RefreshAsync();
    }

    [RelayCommand]
    private async Task RunCode()
    {
        if (string.IsNullOrWhiteSpace(LuaCode))
        {
            ShowValidationError("Lua 代码不能为空。");
            return;
        }

        await RunLuaCommandAsync(
            "执行代码",
            (client, cancellationToken) => client.Lua.ExecuteAsync(LuaCode, MapLuaState, cancellationToken));
    }

    [RelayCommand]
    private async Task RunFile()
    {
        if (!TryGetLuaFilePath(out var fullPath))
        {
            return;
        }

        await RunLuaCommandAsync(
            "执行文件",
            (client, cancellationToken) => client.Lua.ExecuteFileAsync(fullPath, MapLuaState, cancellationToken));
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
        StatusText = $"{actionName}中...";
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
            StatusText = $"{actionName}失败：无法连接 Ra3Hacker API，请先在工具箱点击“打开调试工具”，并确认 RA3 已启动。";
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
        ReturnsText = FormatLines(response.Returns, "(无返回值)");
        OutputText = FormatLines(response.Output, "(无输出)");
        ResponseText = JsonSerializer.Serialize(response, JsonOptions);

        var sourceFileText = string.IsNullOrWhiteSpace(response.SourceFile)
            ? ""
            : $"，文件：{response.SourceFile}";
        var resultText = response.Ok ? "完成" : "失败";
        StatusText = $"{actionName}{resultText}：环境=地图lua，ResultCode={response.ResultCode}{sourceFileText}";

        if (!response.Ok && !string.IsNullOrWhiteSpace(response.Message))
        {
            StatusText += $"，{response.Message}";
        }
    }

    private bool TryGetLuaFilePath(out string fullPath)
    {
        fullPath = "";

        if (string.IsNullOrWhiteSpace(LuaFilePath))
        {
            ShowValidationError("Lua 文件路径不能为空。");
            return false;
        }

        try
        {
            fullPath = Path.GetFullPath(LuaFilePath.Trim());
        }
        catch (Exception ex)
        {
            ShowValidationError("Lua 文件路径无效：" + ex.Message);
            return false;
        }

        if (!File.Exists(fullPath))
        {
            ShowValidationError("Lua 文件不存在：" + fullPath);
            return false;
        }

        return true;
    }

    private void ShowValidationError(string message)
    {
        StatusText = message;
        OutputText = message;
        ReturnsText = "";
        ResponseText = "";
    }

    private static string FormatLines(IReadOnlyList<string> lines, string emptyText)
    {
        return lines.Count == 0
            ? emptyText
            : string.Join(Environment.NewLine, lines);
    }

    private static void TryApplyDialogDirectory(OpenFileDialog dialog, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
                dialog.FileName = Path.GetFileName(fullPath);
            }
        }
        catch
        {
            dialog.FileName = path;
        }
    }

    private async Task SaveCompletionOptionsAsync()
    {
        await _luaCompletionService.UpdateOptionsAsync(
            new LuaCompletionOptions(EnableLuaLibrary, UserCodeLibraryPath));
    }

    private void OnCompletionIndexChanged(object? sender, LuaCompletionIndexChangedEventArgs e)
    {
        void ApplyStatus() => CompletionStatusText = e.Status.Message;

        if (App.Current.Dispatcher.CheckAccess())
        {
            ApplyStatus();
        }
        else
        {
            App.Current.Dispatcher.BeginInvoke(ApplyStatus);
        }
    }

    public void Dispose()
    {
        _luaCompletionService.IndexChanged -= OnCompletionIndexChanged;
    }
}
