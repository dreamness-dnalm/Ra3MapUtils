using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class AIPage : Page
{
    public AIPageViewModel _aIPageViewModel { get => (AIPageViewModel)DataContext; }
    
    public AIPage()
    {
        DataContext = App.Current.Services.GetRequiredService<AIPageViewModel>();
        InitializeComponent();
    }
}