using System.Windows;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class LogViewerWindow : FluentWindow
{
    public LogViewerWindowViewModel _LogViewerWindowViewModel {get => (LogViewerWindowViewModel)DataContext;}
    
    public LogViewerWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<LogViewerWindowViewModel>();
        InitializeComponent();
        _LogViewerWindowViewModel._logViewerWindow = this;
    }
    
}