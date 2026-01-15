using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dreamness.ScriptExecutor;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.ViewModels.SubWindows;
using Ra3MapUtils.Views.SubWindows;
using SharedFunctionLib.Business;
using SharedFunctionLib.Utils;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class NanoProgramPageViewModel : ObservableObject
{
    private readonly INanoProgramService _nanoProgramService = App.Current.Services.GetRequiredService<INanoProgramService>();

    public ObservableCollection<NanoProgramListItemViewModel> NanoPrograms { get; } = new();

    public ListCollectionView NanoProgramsView { get; }

    [ObservableProperty] private NanoProgramListItemViewModel _selectedProgram;

    [ObservableProperty] private bool _hasSelection;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _onlyEnabled;

    [ObservableProperty] private bool _onlyWbVisible = true;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _statusText = "就绪";

    public NanoProgramPageViewModel()
    {
        NanoProgramsView = (ListCollectionView)CollectionViewSource.GetDefaultView(NanoPrograms);
        NanoProgramsView.Filter = FilterNanoPrograms;
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

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.ToLowerInvariant();
        return (item.Name?.ToLowerInvariant().Contains(keyword) ?? false)
               || (item.Description?.ToLowerInvariant().Contains(keyword) ?? false)
               || (item.Id?.ToLowerInvariant().Contains(keyword) ?? false);
    }

    partial void OnSearchTextChanged(string value) => NanoProgramsView.Refresh();

    partial void OnOnlyEnabledChanged(bool value) => NanoProgramsView.Refresh();

    partial void OnOnlyWbVisibleChanged(bool value) => NanoProgramsView.Refresh();

    partial void OnSelectedProgramChanged(NanoProgramListItemViewModel value)
    {
        HasSelection = value != null;
    }

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
            StatusText = "正在加载微程序...";
            NanoPrograms.Clear();

            var programs = await Task.Run(() => _nanoProgramService.GetNanoPrograms());
            foreach (var program in programs.OrderBy(p => p.Order))
            {
                var item = new NanoProgramListItemViewModel(program);
                HookItem(item);
                NanoPrograms.Add(item);
            }

            ReindexAndPersistOrder();
            NanoProgramsView.Refresh();
            StatusText = $"已加载 {NanoPrograms.Count} 个微程序";
        }
        catch (System.Exception ex)
        {
            StatusText = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Create()
    {
        MessageBox.Show("这里可以弹出创建微程序的对话框，当前仅提供UI占位。", "新建微程序");
    }

    [RelayCommand]
    private void Import()
    {
        MessageBox.Show("这里可以接入导入逻辑，当前仅提供UI占位。", "导入微程序");
    }

    [RelayCommand]
    private void OpenUserNanoProgramFolder()
    {
        var path = Path.Combine(Ra3MapUtilsPathUtil.UserDataPath, "nano_programs", "user");
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
            StatusText = $"已打开自定义微程序目录：{path}";
        }
        catch (System.Exception ex)
        {
            StatusText = $"打开自定义微程序目录失败：{ex.Message}";
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
            StatusText = $"{item.Name} 已被禁用，无法运行";
            return;
        }

        try
        {
            item.IsRunning = true;
            StatusText = $"正在运行 {item.Name}";
            var scriptExecutionResult = await Task.Run(() => _nanoProgramService.ExecuteNanoProgram(item.Id, new System.Collections.Generic.Dictionary<string, string>()));
            if (scriptExecutionResult.Success)
            {
                StatusText = $"运行完成：{item.Name}";
            }
            else
            {
                StatusText = $"运行失败：{item.Name}，错误：{scriptExecutionResult.Error}";
            }
            ShowRunResultWindow(item, scriptExecutionResult);
        }
        catch (System.Exception ex)
        {
            StatusText = $"运行失败：{ex.Message}";
            ShowRunResultWindow(item, null, ex);
        }
        finally
        {
            item.IsRunning = false;
        }
    }

    [RelayCommand]
    private void Stop(NanoProgramListItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        // 暂无后台执行管理，仅更新标记用于UI展示
        item.IsRunning = false;
        StatusText = $"已尝试中止 {item.Name}";
    }

    [RelayCommand]
    private async Task RunSelected()
    {
        await Run(SelectedProgram);
    }

    [RelayCommand]
    private void StopSelected()
    {
        Stop(SelectedProgram);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ToggleRun(NanoProgramListItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        if (item.IsRunning)
        {
            return;
        }

        if (!item.IsEnabled)
        {
            StatusText = $"{item.Name} 已被禁用，无法运行";
            return;
        }

        await Run(item);
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
            StatusText = $"路径不存在：{path}";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
            StatusText = $"已打开文件夹：{path}";
        }
        catch (System.Exception ex)
        {
            StatusText = $"打开文件夹失败：{ex.Message}";
        }
    }

    [RelayCommand]
    private void Delete(NanoProgramListItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        NanoPrograms.Remove(item);
        ReindexAndPersistOrder();
        NanoProgramsView.Refresh();
        StatusText = $"已删除 {item.Name}";
    }

    public void MoveItem(NanoProgramListItemViewModel source, NanoProgramListItemViewModel target)
    {
        if (source == null || target == null || source == target)
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
        StatusText = $"已调整 {source.Name} 顺序";
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

        if (e.PropertyName == nameof(NanoProgramListItemViewModel.IsEnabled)
            || e.PropertyName == nameof(NanoProgramListItemViewModel.IsWbVisible))
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
        NanoProgramMetaBusiness.AddOrUpdate(item.Id, item.IsEnabled, item.IsWbVisible, item.Order);
    }

    private void ShowRunResultWindow(NanoProgramListItemViewModel item, ScriptExecutionResult? result, System.Exception? ex = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = new NanoProgramRunResultWindowViewModel(item.Name, result, ex);
            var window = new NanoProgramRunResultWindow(vm);
            window.Show();
        });
    }
}

public partial class NanoProgramListItemViewModel : ObservableObject
{
    public NanoProgramModel Model { get; }

    public string Id => Model.Info.ID;

    public string Name => Model.Info.Name;

    public string Description => Model.Info.Description;

    public string InstallTypeLabel => Model.Info.InstallType switch
    {
        NanoProgramInstallType.Official => "内置",
        NanoProgramInstallType.User => "自定义",
        NanoProgramInstallType.Store => "商店",
        _ => "未知"
    };

    [ObservableProperty] private bool _isEnabled;

    [ObservableProperty] private bool _isWbVisible;

    [ObservableProperty] private int _order;

    [ObservableProperty] private bool _isRunning;

    public NanoProgramListItemViewModel(NanoProgramModel model)
    {
        Model = model;
        _isEnabled = model.IsEnabled;
        _isWbVisible = model.IsWbVisible;
        _order = model.Order;
    }

    partial void OnIsEnabledChanged(bool value) => Model.IsEnabled = value;

    partial void OnIsWbVisibleChanged(bool value) => Model.IsWbVisible = value;

    partial void OnOrderChanged(int value) => Model.Order = value;
}