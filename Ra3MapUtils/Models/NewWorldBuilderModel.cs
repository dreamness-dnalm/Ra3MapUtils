using CommunityToolkit.Mvvm.ComponentModel;
using hospital_pc_client.Utils;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class NewWorldBuilderModel: ObservableObject, INotify
{
    [ObservableProperty] private bool _isPluginsInstalled;
    
    [ObservableProperty] private bool _isPluginsInstalling;

    [ObservableProperty] private bool _isPluginsInstallError;
    
    partial void OnIsPluginsInstalledChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("NewWorldBuilderModelChanged"));
    }
    
    partial void OnIsPluginsInstallingChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("NewWorldBuilderModelChanged"));
    }
    
    partial void OnIsPluginsInstallErrorChanged(bool value)
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("NewWorldBuilderModelChanged"));
    }

    // public void Init()
    // {
    //     ifNewWorldBuilderBusiness.IsNewWorldBuilderPathValid
    // }
    
    public List<IObserver> _observers { get; set; } = new List<IObserver>();
}