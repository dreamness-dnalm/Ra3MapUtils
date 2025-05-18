using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows;

public partial class BorderManagerWindow : FluentWindow
{
    public BorderManagerWindowViewModel _borderManagerWindowViewModel { get => (BorderManagerWindowViewModel)DataContext; }
    
    public BorderManagerWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<BorderManagerWindowViewModel>();
        InitializeComponent();
    }
} 