using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class NanoProgramPage : Page
{
    public NanoProgramPageViewModel _nanoProgramPageViewModel => (NanoProgramPageViewModel)DataContext;

    public NanoProgramPage()
    {
        DataContext = App.Current.Services.GetRequiredService<Ra3MapUtils.ViewModels.MainWindowPages.NanoProgramPageViewModel>();
        InitializeComponent();
    }
}