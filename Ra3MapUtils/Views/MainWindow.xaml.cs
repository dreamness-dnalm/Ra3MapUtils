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
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Ra3MapUtils.Views.MainWindowPages;

namespace Ra3MapUtils;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel _mainWindowViewModel { get => (MainWindowViewModel)DataContext; }

    public MainWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>();
        InitializeComponent();
        Loaded += (_, _) => MainNavigationView.Navigate("HomePage");
    }
}