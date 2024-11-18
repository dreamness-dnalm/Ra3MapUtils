using CommunityToolkit.Mvvm.ComponentModel;
using hospital_pc_client.Utils;
using Velopack;

namespace Ra3MapUtils.Models;

public partial class UpdateModel: ObservableObject, INotify
{
    [ObservableProperty] private string _latestVersionStr;

    public UpdateInfo UpdateInfo;
    
    [ObservableProperty] private bool _isLatestVersion;

    // [ObservableProperty] private bool _isBetaChannel;

    [ObservableProperty] private bool _isAutoUpdate;
    
    [ObservableProperty] private bool _isCheckingUpdate;
    
    [ObservableProperty] private bool _isUpdateAvailable;

    [ObservableProperty] private bool _isCheckingUpdateError;
    
    [ObservableProperty] private bool _isDownloadingUpdate;

    [ObservableProperty] private bool _isDownloadUpdateFinished;
    
    [ObservableProperty] private bool _isDownloadUpdateError;

    [ObservableProperty] private bool _isAreadyUpdated;
    
    [ObservableProperty] private int _downloadProgress;

    [ObservableProperty] private string _releaseNotesHtml;
    
    public List<IObserver> _observers { get; set; } = new List<IObserver>();

    partial void OnLatestVersionStrChanged(string value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsLatestVersionChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsCheckingUpdateChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsUpdateAvailableChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsCheckingUpdateErrorChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsDownloadingUpdateChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsDownloadUpdateFinishedChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsDownloadUpdateErrorChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnIsAreadyUpdatedChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnDownloadProgressChanged(int value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
    partial void OnReleaseNotesHtmlChanged(string value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("UpdateModelChanged"));
    }
    
}