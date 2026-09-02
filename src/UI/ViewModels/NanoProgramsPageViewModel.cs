using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.NanoPrograms;
using Core.Paths;
using UI.Services;
using UI.Views.Dialogs;

namespace UI.ViewModels;

public partial class NanoProgramsPageViewModel : ObservableObject
{
    private readonly INanoProgramService _nanoProgramService;
    private readonly ILocalizationService _localization;

    public ObservableCollection<NanoProgramListItemViewModel> NanoPrograms { get; } = new();

    public ListCollectionView NanoProgramsView { get; }

    [ObservableProperty] private NanoProgramListItemViewModel? _selectedProgram;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _onlyEnabled;

    [ObservableProperty] private bool _onlyWbVisible;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _statusText = "";

    public NanoProgramsPageViewModel(
        INanoProgramService nanoProgramService,
        ILocalizationService localization)
    {
        _nanoProgramService = nanoProgramService;
        _localization = localization;
        StatusText = _localization.GetString("Nano_Ready");
        NanoProgramsView = (ListCollectionView)CollectionViewSource.GetDefaultView(NanoPrograms);
        NanoProgramsView.Filter = FilterNanoPrograms;
        _localization.LanguageChanged += (_, _) =>
        {
            foreach (var item in NanoPrograms)
            {
                item.RefreshLocalized();
            }

            NanoProgramsView.Refresh();
            if (!IsBusy)
            {
                StatusText = _localization.GetString("Nano_Ready");
            }
        };
        _ = Refresh();
    }

    private bool FilterNanoPrograms(object obj)
    {
        if (obj is not NanoProgramListItemViewModel item)
        {
            return false;
        }

        if (OnlyEnabled && !item.IsEnabled)
        {
            return false;
        }

        if (OnlyWbVisible && !item.IsWbVisible)
        {
            return false;
        }

        return NanoProgramDisplayResolver.MatchesSearch(item.Model.Info, SearchText);
    }

    partial void OnSearchTextChanged(string value) => NanoProgramsView.Refresh();

    partial void OnOnlyEnabledChanged(bool value) => NanoProgramsView.Refresh();

    partial void OnOnlyWbVisibleChanged(bool value) => NanoProgramsView.Refresh();

    [RelayCommand]
    private async Task Refresh()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusText = _localization.GetString("Nano_Loading");
            NanoPrograms.Clear();

            var programs = await Task.Run(() => _nanoProgramService.GetNanoPrograms());
            foreach (var program in programs.OrderBy(p => p.Order))
            {
                var item = new NanoProgramListItemViewModel(program, _localization);
                HookItem(item);
                NanoPrograms.Add(item);
            }

            ReindexAndPersistOrder();
            NanoProgramsView.Refresh();
            StatusText = string.Format(_localization.GetString("Nano_LoadedFormat"), NanoPrograms.Count);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(_localization.GetString("Nano_LoadFailedFormat"), ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenUserNanoProgramFolder()
    {
        var path = AppDataPaths.UserNanoProgramsPath;
        try
        {
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true,
            });
            StatusText = string.Format(_localization.GetString("Nano_OpenedUserFolderFormat"), path);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(_localization.GetString("Nano_OpenUserFolderFailedFormat"), ex.Message);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Run(NanoProgramListItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        if (!item.IsEnabled)
        {
            StatusText = string.Format(_localization.GetString("Nano_DisabledFormat"), item.Name);
            return;
        }

        try
        {
            item.IsRunning = true;
            StatusText = string.Format(_localization.GetString("Nano_RunningFormat"), item.Name);
            // Legacy-compatible: empty args; scripts may prompt for MapFilePath themselves.
            var result = await Task.Run(() =>
                _nanoProgramService.ExecuteNanoProgram(item.Id, new Dictionary<string, string>()));

            StatusText = result.Success
                ? string.Format(_localization.GetString("Nano_RunOkFormat"), item.Name)
                : string.Format(_localization.GetString("Nano_RunFailFormat"), item.Name, result.Error);
            ShowRunResult(item.Name, result, null);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(_localization.GetString("Nano_RunExceptionFormat"), ex.Message);
            ShowRunResult(item.Name, null, ex);
        }
        finally
        {
            item.IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task RunSelected() => await Run(SelectedProgram);

    public void MoveItem(NanoProgramListItemViewModel source, NanoProgramListItemViewModel target)
    {
        if (source == null || target == null || ReferenceEquals(source, target))
        {
            return;
        }

        var oldIndex = NanoPrograms.IndexOf(source);
        var newIndex = NanoPrograms.IndexOf(target);
        if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
        {
            return;
        }

        NanoPrograms.Move(oldIndex, newIndex);
        ReindexAndPersistOrder();
        NanoProgramsView.Refresh();
        StatusText = string.Format(_localization.GetString("Nano_ReorderedFormat"), source.Name);
    }

    [RelayCommand]
    private void OpenFolder(NanoProgramListItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        var path = item.Model.Info.Path;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            StatusText = string.Format(_localization.GetString("Nano_PathMissingFormat"), path);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true,
            });
            StatusText = string.Format(_localization.GetString("Nano_OpenedFolderFormat"), path);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(_localization.GetString("Nano_OpenFolderFailedFormat"), ex.Message);
        }
    }

    private void HookItem(NanoProgramListItemViewModel item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NanoProgramListItemViewModel item)
        {
            return;
        }

        if (e.PropertyName is nameof(NanoProgramListItemViewModel.IsEnabled)
            or nameof(NanoProgramListItemViewModel.IsWbVisible))
        {
            PersistSingle(item);
        }
    }

    private void ReindexAndPersistOrder()
    {
        for (var i = 0; i < NanoPrograms.Count; i++)
        {
            NanoPrograms[i].Order = i;
            PersistSingle(NanoPrograms[i]);
        }
    }

    private void PersistSingle(NanoProgramListItemViewModel item)
    {
        _nanoProgramService.SaveMeta(item.Id, item.IsEnabled, item.IsWbVisible, item.Order);
    }

    private static void ShowRunResult(string programName, NanoProgramExecutionResult? result, Exception? ex)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new NanoProgramRunResultDialog(programName, result, ex)
            {
                Owner = Application.Current.MainWindow,
            };
            dialog.Show();
        });
    }
}

public partial class NanoProgramListItemViewModel : ObservableObject
{
    private readonly ILocalizationService _localization;

    public NanoProgramModel Model { get; }

    public string Id => Model.Info.ID;

    [ObservableProperty] private string _name = "";

    [ObservableProperty] private string _description = "";

    [ObservableProperty] private string _installTypeLabel = "";

    [ObservableProperty] private bool _isEnabled;

    [ObservableProperty] private bool _isWbVisible;

    [ObservableProperty] private int _order;

    [ObservableProperty] private bool _isRunning;

    public NanoProgramListItemViewModel(NanoProgramModel model, ILocalizationService localization)
    {
        Model = model;
        _localization = localization;
        _isEnabled = model.IsEnabled;
        _isWbVisible = model.IsWbVisible;
        _order = model.Order;
        RefreshLocalized();
    }

    public void RefreshLocalized()
    {
        var lang = _localization.CurrentLanguage;
        Name = NanoProgramDisplayResolver.ResolveName(Model.Info, lang);
        Description = NanoProgramDisplayResolver.ResolveDescription(Model.Info, lang);
        InstallTypeLabel = Model.Info.InstallType switch
        {
            NanoProgramInstallType.Official => _localization.GetString("Nano_TypeOfficial"),
            NanoProgramInstallType.User => _localization.GetString("Nano_TypeUser"),
            NanoProgramInstallType.Store => _localization.GetString("Nano_TypeStore"),
            _ => _localization.GetString("Nano_TypeUnknown"),
        };
    }

    partial void OnIsEnabledChanged(bool value) => Model.IsEnabled = value;

    partial void OnIsWbVisibleChanged(bool value) => Model.IsWbVisible = value;

    partial void OnOrderChanged(int value) => Model.Order = value;
}
