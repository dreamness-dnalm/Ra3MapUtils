using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ViewModels.toolbox;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SharedFunctionLib.Business;
using MessageBox = System.Windows.Forms.MessageBox;
using WpfApplication = System.Windows.Application;
using WpfWindow = System.Windows.Window;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ToolBoxPageViewModel: ObservableObject
{
    private SettingPageViewModel _settingPageViewModel = App.Current.Services.GetRequiredService<SettingPageViewModel>();
    // private LogViewerWindow _logViewerWindow = App.Current.Services.GetRequiredService<LogViewerWindow>();
    
    [RelayCommand]
    private void OpenLogViewerWindow()
    {
        if ((!NewWorldBuilderBusiness.IsNewWorldBuilderPathValid) ||
            (!_settingPageViewModel.NewWorldBuilderModel.IsPluginsInstalled))
        {
            MessageBox.Show("请配置新地编联动, 在\"设置\"->\"新地编联动\"中设置");
            return;
        }
        
        if (GlobalVarsModel.LogViewerWindowOpened)
        {
            // MessageBox.Show("日志查看工具已经打开");
            return;
        }
        LogViewerWindow _logViewerWindow = App.Current.Services.GetRequiredService<LogViewerWindow>();
        _logViewerWindow._LogViewerWindowViewModel.OnLoad();
        _logViewerWindow.Show();

        GlobalVarsModel.LogViewerWindowOpened = true;
    }
    
    [RelayCommand]
    private void OpenChatLuaHelperWindow()
    {
        if(GlobalVarsModel.ChatLuaHelperWindowOpened)
        {
            // MessageBox.Show("地图聊天框Lua拆分工具已经打开");
            return;
        }
        
        
        ChatLuaHelperViewWindow _chatLuaHelperViewWindow = App.Current.Services.GetRequiredService<ChatLuaHelperViewWindow>();
        _chatLuaHelperViewWindow._chatLuaHelperViewModel.OnLoad();
        _chatLuaHelperViewWindow.Show();
        if (_chatLuaHelperViewWindow._chatLuaHelperViewModel.ChatLuaHelperModel.ChatLuaHelperFilePath != "")
        {
            _chatLuaHelperViewWindow._chatLuaHelperViewModel.ReloadFile();
        }
        
        GlobalVarsModel.ChatLuaHelperWindowOpened = true;
    }

    [RelayCommand]
    private void OpenMapDataEditorWindow()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "选择地图数据文件",
            Filter = "地图数据文件|*.map;*.scb;*.bin",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var filePath = openFileDialog.FileName;
        if (!File.Exists(filePath))
        {
            MessageBox.Show("文件不存在: " + filePath);
            return;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension is not (".map" or ".scb" or ".bin"))
        {
            MessageBox.Show("不支持的文件类型: " + extension);
            return;
        }

        var normalizedFilePath = TryNormalizePath(filePath);
        if (normalizedFilePath is null)
        {
            MessageBox.Show("Failed to normalize path: " + filePath);
            return;
        }

        var existingWindow = WpfApplication.Current.Windows
            .OfType<MapDataEditorWindow>()
            .FirstOrDefault(window => IsMapDataEditorWindowForPath(window, normalizedFilePath));
        if (existingWindow is not null)
        {
            ActivateWindow(existingWindow);
            return;
        }

        var mapDataEditorWindow = App.Current.Services.GetRequiredService<MapDataEditorWindow>();
        if (!mapDataEditorWindow._mapDataEditorWindowViewModel.LoadMapDataFromFile(filePath))
        {
            return;
        }

        mapDataEditorWindow.Show();
    }

    private static bool IsMapDataEditorWindowForPath(MapDataEditorWindow window, string normalizedFilePath)
    {
        var existingPath = TryNormalizePath(window._mapDataEditorWindowViewModel.FilePath);
        return existingPath is not null &&
               string.Equals(existingPath, normalizedFilePath, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryNormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }
    }

    private static void ActivateWindow(WpfWindow window)
    {
        if (window.WindowState == System.Windows.WindowState.Minimized)
        {
            window.WindowState = System.Windows.WindowState.Normal;
        }

        window.Activate();
    }

    [RelayCommand]
    private void OpenMoreFunctionsWindow()
    {
        MessageBox.Show("欢迎加入QQ群: 513118543");
    }
}

