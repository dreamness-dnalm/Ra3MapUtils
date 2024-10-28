using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class MapManagePage : Page
{
    public MapManagePageViewModel _mapManagePageViewModel { get => (MapManagePageViewModel)DataContext; }
    
    public MapManagePage()
    {
        DataContext = App.Current.Services.GetRequiredService<MapManagePageViewModel>();
        InitializeComponent();
        _mapManagePageViewModel.RefreshMapListCommand.Execute(null);
    }
}