using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.WinForms;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject, IObserver, INotify
{
    private IUpdateService _updateService = App.Current.Services.GetRequiredService<IUpdateService>();

    [ObservableProperty] private UpdateModel _updateModel;

    [ObservableProperty] private Visibility _updateNowVisibility = Visibility.Collapsed;

    [ObservableProperty] private string _versionStr = GlobalVarsModel.VersionStr;
    
    [ObservableProperty] private bool _updateNowEnabled;
    
    [ObservableProperty] private bool _checkUpdateEnabled;

    [ObservableProperty] private bool _isUpdating;

    [ObservableProperty] private string _updateHint;

    [ObservableProperty] private Brush _updateHintColor;
    
    public WebBrowser UpdateReleaseNotesViewer;

    public void OnLoadUpdatePart()
    {
        _updateModel = new UpdateModel();
        ObservableUtil.Subscribe(_updateModel, this);
    }

    public async Task ReflushUpdateReleaseNotesViewer()
    {
        if (UpdateReleaseNotesViewer != null && UpdateModel.ReleaseNotesHtml != null)
        {
            UpdateReleaseNotesViewer.NavigateToString("<head><meta charset=\"utf-8\"></head>" + UpdateModel.ReleaseNotesHtml);
        }
    }

    partial void OnUpdateModelChanged(UpdateModel value)
    {
        ReflushUpdateReleaseNotesViewer();
        
        if (value.IsAreadyUpdated)
        {
            return;
        }
        
        if (value.IsDownloadUpdateFinished)
        {
            var taskDialog = new TaskDialog();
            taskDialog.WindowTitle = "是否立即更新?";
            taskDialog.MainInstruction = "更新完成,是否立即重启?";
            taskDialog.Buttons.Add(new TaskDialogButton("立即重启"));
            taskDialog.Buttons.Add(new TaskDialogButton("退出后更新"));
            taskDialog.Buttons.Add(new TaskDialogButton("不更新"));

            var selectedButtonText = taskDialog.ShowDialog().Text;
            if(selectedButtonText == "立即重启")
            {
                _updateService.UpdateAndRestart(value);
                
                UpdateHint = "更新完成,等待重启";
                UpdateHintColor = Brushes.Green;
            }
            else if(selectedButtonText == "退出后更新")
            {
                _updateService.WaitExitAnUpdate(value);
                UpdateHint = "更新完成,等待重启";
                UpdateHintColor = Brushes.Green;
            }
            else
            {
                UpdateHint = "已放弃更新";
                UpdateHintColor = Brushes.PaleVioletRed;
                
            }
            return;
        }

        if (value.IsDownloadingUpdate)
        {
            UpdateHint = $"下载更新中... {value.DownloadProgress}%";
            UpdateHintColor = Brushes.CornflowerBlue;
            
            return;
        }
        
        if (value.IsDownloadUpdateError)
        {
            UpdateHint = "下载更新失败, 请稍候再试";
            UpdateHintColor = Brushes.PaleVioletRed;
            return;
        }

        if (value.IsCheckingUpdateError)
        {
            UpdateHint = "检查更新失败, 请稍候再试";
            UpdateHintColor = Brushes.PaleVioletRed;
            UpdateNowEnabled = false;
            UpdateNowVisibility = Visibility.Collapsed;
            return;
        }
        
        if(value.IsCheckingUpdate)
        {
            UpdateHint = "正在检查更新...";
            UpdateHintColor = Brushes.Gray;
            UpdateNowEnabled = false;
            UpdateNowVisibility = Visibility.Collapsed;
            return;
        }
        
        if (value.IsUpdateAvailable)
        {
            UpdateHint = "有新版本可用: " + value.LatestVersionStr;
            UpdateHintColor = Brushes.Green;
            UpdateNowEnabled = true;
            UpdateNowVisibility = Visibility.Visible;
            
            return;
        }
        
        UpdateHint = "当前已是最新版本";
        UpdateHintColor = Brushes.Green;
    }
    
    [RelayCommand]
    private async Task<bool> CheckUpdate()
    {
        return await _updateService.FillUpdateModel(UpdateModel);
    }

    [RelayCommand]
    private async Task<bool> DownloadUpdate()
    {
        return await _updateService.DownloadUpdateAsync(UpdateModel);
    }
    
    public async Task<bool> UpdateNow()
    {
        await CheckUpdate();
        if (SettingModel.IsAutoUpdate && UpdateModel.IsUpdateAvailable)
        {
            await DownloadUpdate();
        }
        return true;
    }

    public List<IObserver> _observers { get; set; } = new List<IObserver>();
}