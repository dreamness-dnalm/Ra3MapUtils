using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using FormsDialogResult = System.Windows.Forms.DialogResult;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class DebuggerMapSettingsWindowViewModel : ObservableObject
{
    private readonly IDebuggerMapSettingsService _settingsService;

    /// <summary>
    /// 创建调试器地图配置窗口视图模型。
    /// </summary>
    public DebuggerMapSettingsWindowViewModel(IDebuggerMapSettingsService settingsService)
    {
        _settingsService = settingsService;
        ReloadSettingsCore();
    }

    /// <summary>当前编辑的调试器配置文件路径。</summary>
    public string SettingsFilePath => _settingsService.SettingsFilePath;

    [ObservableProperty] private bool _isTopmost;

    [ObservableProperty] private string _mapFolder = "";

    [ObservableProperty] private bool _hideBuiltInMaps;

    [ObservableProperty] private string _statusText = "";

    [RelayCommand]
    private void BrowseMapFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择游戏加载地图的目录",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(MapFolder) ? MapFolder : "",
        };

        if (dialog.ShowDialog() == FormsDialogResult.OK)
        {
            MapFolder = dialog.SelectedPath;
            StatusText = "已选择地图目录，保存后启动或重启调试器即可生效。";
        }
    }

    [RelayCommand]
    private void UseDefaultMapFolder()
    {
        MapFolder = "";
        StatusText = "地图目录已设为默认值，请保存配置。";
    }

    [RelayCommand]
    private void ReloadSettings()
    {
        ReloadSettingsCore();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        TrySaveSettings();
    }

    [RelayCommand]
    private async Task SaveAndStartDebugger()
    {
        if (!TrySaveSettings())
        {
            return;
        }

        StatusText = "配置已保存，正在启动调试器...";
        try
        {
            var started = await _settingsService.StartDebuggerAsync();
            StatusText = started
                ? "配置已保存并启动调试器。若调试器此前正在运行，请先关闭旧实例。"
                : "配置已保存，但调试器启动失败。";
        }
        catch (Exception ex)
        {
            StatusText = "配置已保存，但调试器启动失败：" + ex.Message;
        }
    }

    private void ReloadSettingsCore()
    {
        try
        {
            ApplySettings(_settingsService.Load());
            StatusText = "已读取当前调试器配置。";
        }
        catch (Exception ex)
        {
            StatusText = "读取配置失败：" + ex.Message;
        }
    }

    private bool TrySaveSettings()
    {
        try
        {
            _settingsService.Save(new DebuggerMapSettingsModel
            {
                MapFolder = MapFolder,
                HideBuiltInMaps = HideBuiltInMaps,
            });

            ApplySettings(_settingsService.Load());
            StatusText = "配置已保存。请启动或重启调试器使设置生效。";
            return true;
        }
        catch (Exception ex)
        {
            StatusText = "保存配置失败：" + ex.Message;
            return false;
        }
    }

    private void ApplySettings(DebuggerMapSettingsModel settings)
    {
        MapFolder = settings.MapFolder;
        HideBuiltInMaps = settings.HideBuiltInMaps;
    }
}
