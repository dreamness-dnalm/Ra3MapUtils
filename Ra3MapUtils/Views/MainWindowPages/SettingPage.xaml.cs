using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class SettingPage : Page
{
    public SettingPageViewModel _settingPageViewModel { get => (SettingPageViewModel)DataContext; }
    
    public SettingPage()
    {
        DataContext = App.Current.Services.GetRequiredService<SettingPageViewModel>();
        InitializeComponent();
        _settingPageViewModel.UpdateReleaseNotesViewer = UpdateReleaseNotesViewer;
        _settingPageViewModel.ReflushUpdateReleaseNotesViewer();
        // _settingPageViewModel.OnLoadUpdatePart();
    }
}