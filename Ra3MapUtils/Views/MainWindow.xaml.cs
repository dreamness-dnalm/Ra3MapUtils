using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Ra3MapUtils.Views.MainWindowPages;
using Wpf.Ui.Controls;

namespace Ra3MapUtils;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindowViewModel _mainWindowViewModel { get => (MainWindowViewModel)DataContext; }

    public MainWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>();
        InitializeComponent();
        Loaded += (_, _) => MainNavigationView.Navigate("HomePage");
        ObservableUtil.Subscribe(_mainWindowViewModel._settingPageViewModel, _mainWindowViewModel);
    }
}