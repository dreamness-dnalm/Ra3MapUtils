using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject, IObserver, INotify
{
    [RelayCommand]
    private async Task<bool> SwitchApiServiceEnabledAsync()
    {
        // if (SettingModel.IsEnableApiService)
        // {
        //     App._apiService.Start();
        // }
        // else
        // {
        //     App._apiService.Stop();
        // }
        return true;
    }

    [RelayCommand]
    private async Task<bool> ApplyApiServicePortAsync()
    {
        // todo 

        return true;
    }
    
    
    
}