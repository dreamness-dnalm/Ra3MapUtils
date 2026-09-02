using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Debugger;
using UI.Services;

namespace UI.ViewModels.Tools;

public partial class DebuggerMapSettingsWindowViewModel : ObservableObject
{
    private readonly IGameDebuggerService _debugger;
    private readonly ILocalizationService _localization;

    public DebuggerMapSettingsWindowViewModel(
        IGameDebuggerService debugger,
        ILocalizationService localization)
    {
        _debugger = debugger;
        _localization = localization;
        RefreshLocalized();
        ReloadSettings();
        _localization.LanguageChanged += (_, _) => RefreshLocalized();
    }

    [ObservableProperty]
    private bool _isTopmost;

    [ObservableProperty]
    private string _windowTitle = "";

    [ObservableProperty]
    private string _restartWarning = "";

    [ObservableProperty]
    private string _mapFolderLabel = "";

    [ObservableProperty]
    private string _mapFolderHint = "";

    [ObservableProperty]
    private string _hideBuiltInLabel = "";

    [ObservableProperty]
    private string _browseLabel = "";

    [ObservableProperty]
    private string _useDefaultLabel = "";

    [ObservableProperty]
    private string _saveLabel = "";

    [ObservableProperty]
    private string _mapFolder = "";

    [ObservableProperty]
    private bool _hideBuiltInMaps;

    [ObservableProperty]
    private string _statusText = "";

    [RelayCommand]
    private void BrowseMapFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = _localization.GetString("DbgMap_BrowseTitle", "选择游戏加载地图的目录"),
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(MapFolder) ? MapFolder : "",
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        MapFolder = dialog.SelectedPath;
        StatusText = _localization.GetString("DbgMap_FolderSelected", "已选择地图目录，保存后启动或重启调试器即可生效。");
    }

    [RelayCommand]
    private void UseDefaultMapFolder()
    {
        MapFolder = "";
        StatusText = _localization.GetString("DbgMap_DefaultSet", "地图目录已设为默认值，请保存配置。");
    }

    [RelayCommand]
    private void ReloadSettings()
    {
        try
        {
            var settings = _debugger.LoadMapSettings();
            MapFolder = settings.MapFolder;
            HideBuiltInMaps = settings.HideBuiltInMaps;
            StatusText = _localization.GetString("DbgMap_Loaded", "已读取当前调试器配置。");
        }
        catch (Exception ex)
        {
            StatusText = string.Format(
                _localization.GetString("DbgMap_LoadFailed", "读取配置失败：{0}"),
                ex.Message);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            _debugger.SaveMapSettings(new DebuggerMapSettings
            {
                MapFolder = MapFolder,
                HideBuiltInMaps = HideBuiltInMaps,
            });
            StatusText = _localization.GetString("DbgMap_Saved", "配置已保存。若调试器正在运行，请重启后生效。");
        }
        catch (Exception ex)
        {
            StatusText = string.Format(
                _localization.GetString("DbgMap_SaveFailed", "保存失败：{0}"),
                ex.Message);
        }
    }

    private void RefreshLocalized()
    {
        WindowTitle = _localization.GetString("DbgMap_Title", "重设游戏地图路径");
        RestartWarning = _localization.GetString(
            "DbgMap_RestartWarning",
            "配置仅在调试器启动时读取。若调试器正在运行，请先关闭旧实例，再保存并重新启动。");
        MapFolderLabel = _localization.GetString("DbgMap_FolderLabel", "游戏加载地图目录");
        MapFolderHint = _localization.GetString("DbgMap_FolderHint", "留空时使用游戏默认的地图目录。");
        HideBuiltInLabel = _localization.GetString("DbgMap_HideBuiltIn", "隐藏所有官方地图");
        BrowseLabel = _localization.GetString("DbgMap_Browse", "选择目录");
        UseDefaultLabel = _localization.GetString("DbgMap_UseDefault", "使用默认");
        SaveLabel = _localization.GetString("DbgMap_Save", "保存配置");
    }
}
