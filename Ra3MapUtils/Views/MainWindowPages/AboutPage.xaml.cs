using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class AboutPage : Page
{
    public AboutPageViewModel _aboutPageViewModel { get => (AboutPageViewModel)DataContext; }
    
    public AboutPage()
    {
        DataContext = App.Current.Services.GetRequiredService<AboutPageViewModel>();
        InitializeComponent();
        _aboutPageViewModel._aboutPage = this;
    }
}