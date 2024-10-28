using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class ToolBoxPage : Page
{
    public ToolBoxPageViewModel _toolBoxPageViewModel { get => (ToolBoxPageViewModel)DataContext; }
    
    public ToolBoxPage()
    {
        DataContext = App.Current.Services.GetRequiredService<ToolBoxPageViewModel>();
        InitializeComponent();
    }
}