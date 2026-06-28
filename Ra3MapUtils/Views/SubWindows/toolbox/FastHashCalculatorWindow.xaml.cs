using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class FastHashCalculatorWindow : FluentWindow
{
    public FastHashCalculatorWindowViewModel _fastHashCalculatorWindowViewModel
        => (FastHashCalculatorWindowViewModel)DataContext;

    public FastHashCalculatorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<FastHashCalculatorWindowViewModel>();
        InitializeComponent();
    }
}
