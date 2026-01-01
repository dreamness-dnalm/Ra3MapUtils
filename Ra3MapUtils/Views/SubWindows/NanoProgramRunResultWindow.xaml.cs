using System.Windows;
using Ra3MapUtils.ViewModels.SubWindows;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows;

public partial class NanoProgramRunResultWindow : FluentWindow
{
    public NanoProgramRunResultWindow(NanoProgramRunResultWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Owner = Application.Current.MainWindow;
    }
}

