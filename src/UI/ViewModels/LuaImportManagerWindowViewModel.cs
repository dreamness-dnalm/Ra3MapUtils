using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.LuaImport;
using Core.Maps;
using Dreamness.Ra3.Map.Facade.Core;
using Ra3MapUtils.Utils;
using UI.Dialogs;
using UI.Services;

namespace UI.ViewModels;

public partial class LuaImportManagerWindowViewModel : ObservableObject
{
    private readonly ILuaLibConfigStore _configStore;
    private readonly ILuaImportSettingsStore _settingsStore;
    private readonly ILocalizationService _localization;
    private bool _clearedActive;

    public LuaImportManagerWindowViewModel(
        ILuaLibConfigStore configStore,
        ILuaImportSettingsStore settingsStore,
        ILocalizationService localization)
    {
        _configStore = configStore;
        _settingsStore = settingsStore;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RefreshLabels();
        RefreshLabels();
    }

    public ObservableCollection<LuaLibConfigItemViewModel> Libraries { get; } = new();

    [ObservableProperty]
    private string _windowTitle = "Lua 导入";

    [ObservableProperty]
    private string _mapName = "";

    [ObservableProperty]
    private string _mapFilePath = "";

    [ObservableProperty]
    private string _configKey = "";

    [ObservableProperty]
    private LuaLibConfigItemViewModel? _selectedLibrary;

    [ObservableProperty]
    private bool _isTopmost;

    [ObservableProperty]
    private string _statusMessage = "";

    public void Initialize(string mapName, string mapDirectoryPath)
    {
        MapName = mapName;
        var mapFile = Path.Combine(mapDirectoryPath, mapName + ".map");
        MapFilePath = Path.GetFullPath(mapFile);
        ConfigKey = MapFilePath;
        WindowTitle = string.Format(
            _localization.GetString("LuaImport_TitleFormat", "Lua 导入 - {0}"),
            mapName);

        _settingsStore.SetActiveMap(ConfigKey, MapFilePath);
        _clearedActive = false;
        ReloadLibraries();
    }

    public void OnClosed()
    {
        if (!string.IsNullOrWhiteSpace(ConfigKey) && Libraries.Count > 0)
        {
            PersistOrder();
        }

        if (_clearedActive)
        {
            return;
        }

        _settingsStore.ClearActiveMap();
        _clearedActive = true;
    }

    [RelayCommand]
    private void AddLibrary()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = _localization.GetString("LuaImport_SelectLibFolder", "选择 Lua 库目录"),
            UseDescriptionForTitle = true,
        };
        if (dialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            return;
        }

        var showingName = Path.GetFileName(dialog.SelectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(showingName))
        {
            showingName = "LuaLib";
        }

        if (Libraries.Any(l => string.Equals(l.ShowingName, showingName, StringComparison.OrdinalIgnoreCase)))
        {
            showingName += "_" + DateTime.Now.ToString("HHmmss");
        }

        var record = new LuaLibConfigRecord
        {
            MapName = ConfigKey,
            ShowingName = showingName,
            LibPath = dialog.SelectedPath,
            OrderNum = Libraries.Count,
            IsEnabled = 1,
        };
        _configStore.Save(record);
        ReloadLibraries();
        SelectedLibrary = Libraries.LastOrDefault();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RemoveLibrary()
    {
        if (SelectedLibrary is null)
        {
            return;
        }

        var confirm = LocalizedMessageBox.ShowRaw(
            string.Format(
                _localization.GetString("LuaImport_RemoveConfirm", "确认移除库「{0}」？"),
                SelectedLibrary.ShowingName),
            _localization.GetString("LuaImport_Remove", "移除"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        _configStore.Delete(ConfigKey, SelectedLibrary.ShowingName);
        ReloadLibraries();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RenameLibrary()
    {
        if (SelectedLibrary is null)
        {
            return;
        }

        if (!TextInputDialog.TryPrompt(
                GetOwner(),
                _localization.GetString("LuaImport_Rename", "重命名"),
                _localization.GetString("LuaImport_RenamePrompt", "请输入显示名"),
                SelectedLibrary.ShowingName,
                out var newName))
        {
            return;
        }

        newName = newName.Trim();
        if (string.IsNullOrWhiteSpace(newName)
            || string.Equals(newName, SelectedLibrary.ShowingName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            _configStore.Rename(ConfigKey, SelectedLibrary.ShowingName, newName);
            ReloadLibraries();
            SelectedLibrary = Libraries.FirstOrDefault(l =>
                string.Equals(l.ShowingName, newName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            LocalizedMessageBox.ShowRaw(
                ex.Message,
                _localization.GetString("LuaImport_RenameFailed", "重命名失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void ChangePath()
    {
        if (SelectedLibrary is null)
        {
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = _localization.GetString("LuaImport_SelectLibFolder", "选择 Lua 库目录"),
            UseDescriptionForTitle = true,
            SelectedPath = SelectedLibrary.LibPath,
        };
        if (dialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            return;
        }

        var record = new LuaLibConfigRecord
        {
            MapName = ConfigKey,
            ShowingName = SelectedLibrary.ShowingName,
            LibPath = dialog.SelectedPath,
            OrderNum = SelectedLibrary.OrderNum,
            IsEnabled = SelectedLibrary.IsEnabled ? 1 : 0,
        };
        _configStore.Save(record);
        ReloadLibraries();
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp()
    {
        if (SelectedLibrary is null)
        {
            return;
        }

        var index = Libraries.IndexOf(SelectedLibrary);
        if (index <= 0)
        {
            return;
        }

        Libraries.Move(index, index - 1);
        PersistOrder();
        NotifyReorderCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown()
    {
        if (SelectedLibrary is null)
        {
            return;
        }

        var index = Libraries.IndexOf(SelectedLibrary);
        if (index < 0 || index >= Libraries.Count - 1)
        {
            return;
        }

        Libraries.Move(index, index + 1);
        PersistOrder();
        NotifyReorderCommands();
    }

    [RelayCommand]
    private void Import()
    {
        try
        {
            var enabled = Libraries
                .Where(l => l.IsEnabled && !string.IsNullOrWhiteSpace(l.LibPath))
                .Select(l => l.ToRecord(ConfigKey))
                .ToList();
            if (enabled.Count == 0)
            {
                LocalizedMessageBox.ShowRaw(
                    _localization.GetString("LuaImport_NoEnabled", "没有启用的 Lua 库可导入。"),
                    _localization.GetString("LuaImport_Import", "导入"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            PersistOrder();

            var (parent, mapName) = MapPathResolver.ResolveForFacadeOpen(MapFilePath);
            var ra3Map = Ra3MapFacade.Open(parent, mapName);
            MapLuaImporterUtil.ImportLua(ra3Map, enabled);
            StatusMessage = _localization.GetString("LuaImport_ImportDone", "导入完成。");
            LocalizedMessageBox.ShowRaw(
                StatusMessage,
                _localization.GetString("LuaImport_Import", "导入"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LocalizedMessageBox.ShowRaw(
                ex.Message,
                _localization.GetString("LuaImport_ImportFailed", "导入失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenDocs()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/dreamness-dnalm/Ra3MapUtils/wiki",
                UseShellExecute = true,
            });
        }
        catch
        {
            // ignore
        }
    }

    partial void OnSelectedLibraryChanged(LuaLibConfigItemViewModel? value)
    {
        RemoveLibraryCommand.NotifyCanExecuteChanged();
        RenameLibraryCommand.NotifyCanExecuteChanged();
        ChangePathCommand.NotifyCanExecuteChanged();
        NotifyReorderCommands();
    }

    private bool HasSelection() => SelectedLibrary is not null;

    private bool CanMoveUp() => SelectedLibrary is not null && Libraries.IndexOf(SelectedLibrary) > 0;

    private bool CanMoveDown()
    {
        if (SelectedLibrary is null)
        {
            return false;
        }

        var index = Libraries.IndexOf(SelectedLibrary);
        return index >= 0 && index < Libraries.Count - 1;
    }

    private void NotifyReorderCommands()
    {
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    private void ReloadLibraries()
    {
        var selectedName = SelectedLibrary?.ShowingName;
        Libraries.Clear();
        foreach (var record in _configStore.Load(ConfigKey).OrderBy(r => r.OrderNum))
        {
            Libraries.Add(LuaLibConfigItemViewModel.FromRecord(record));
        }

        SelectedLibrary = Libraries.FirstOrDefault(l =>
            string.Equals(l.ShowingName, selectedName, StringComparison.OrdinalIgnoreCase));
        NotifyReorderCommands();
    }

    private void PersistOrder()
    {
        _configStore.ReplaceAll(ConfigKey, Libraries.Select((l, i) =>
        {
            var record = l.ToRecord(ConfigKey);
            record.OrderNum = i;
            return record;
        }));
        for (var i = 0; i < Libraries.Count; i++)
        {
            Libraries[i].OrderNum = i;
        }
    }

    private void RefreshLabels()
    {
        if (!string.IsNullOrWhiteSpace(MapName))
        {
            WindowTitle = string.Format(
                _localization.GetString("LuaImport_TitleFormat", "Lua 导入 - {0}"),
                MapName);
        }
    }

    private static Window? GetOwner() =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        ?? Application.Current?.MainWindow;
}

public partial class LuaLibConfigItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _showingName = "";

    [ObservableProperty]
    private string _libPath = "";

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private bool _isEnabled = true;

    public static LuaLibConfigItemViewModel FromRecord(LuaLibConfigRecord record) => new()
    {
        ShowingName = record.ShowingName,
        LibPath = record.LibPath,
        OrderNum = record.OrderNum,
        IsEnabled = record.IsEnabled > 0,
    };

    public LuaLibConfigRecord ToRecord(string mapName) => new()
    {
        MapName = mapName,
        ShowingName = ShowingName,
        LibPath = LibPath,
        OrderNum = OrderNum,
        IsEnabled = IsEnabled ? 1 : 0,
    };
}
