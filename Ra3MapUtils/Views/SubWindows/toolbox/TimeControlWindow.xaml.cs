using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class TimeControlWindow : FluentWindow
{
    public TimeControlWindowViewModel _timeControlWindowViewModel
        => (TimeControlWindowViewModel)DataContext;

    public TimeControlWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<TimeControlWindowViewModel>();
        InitializeComponent();
    }
}
