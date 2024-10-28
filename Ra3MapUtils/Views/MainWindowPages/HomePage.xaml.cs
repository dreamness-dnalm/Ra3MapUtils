using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class HomePage : Page
{
    public HomePageViewModel _homePageViewModel { get => (HomePageViewModel)DataContext; }
    
    public HomePage()
    {
        DataContext = App.Current.Services.GetRequiredService<HomePageViewModel>();
        InitializeComponent();
    }
}