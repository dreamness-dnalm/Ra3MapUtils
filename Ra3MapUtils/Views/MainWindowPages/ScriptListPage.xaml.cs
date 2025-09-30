using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class ScriptListPage : Page
{
    public ScriptListPageViewModel _scriptListPageViewModel { get => (ScriptListPageViewModel)DataContext; }
    
    public ScriptListPage()
    {
        App.Current.Services.GetRequiredService<ScriptListPageViewModel>();
        InitializeComponent();
    }
}