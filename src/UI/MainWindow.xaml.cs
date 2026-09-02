using System.ComponentModel;
using System.Windows;
using UI.ViewModels;

namespace UI;

public partial class MainWindow
{
    private bool _allowClose;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closing += MainWindow_OnClosing;
    }

    public void ShowFromTray()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    public void AllowClose() => _allowClose = true;

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
    }
}
