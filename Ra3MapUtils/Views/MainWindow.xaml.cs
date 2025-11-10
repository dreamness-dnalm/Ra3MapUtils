using System.ComponentModel;
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
using CommunityToolkit.Mvvm.Messaging;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Messages;
using Ra3MapUtils.ViewModels;
using Ra3MapUtils.Views.MainWindowPages;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

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
        WeakReferenceMessenger.Default.Register<HideWindowMessage>(this, (r, m) =>
        {
            Hide();
        });
        WeakReferenceMessenger.Default.Register<ShowWindowMessage>(this, (r, m) =>
        {
            Show();
            Activate();
        });
        WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (r, m) =>
        {
            // Close();
            // _mutex?.ReleaseMutex();
            Environment.Exit(0);
        });
    }
    
    

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        
        e.Cancel = true;
        Hide();
    }
}