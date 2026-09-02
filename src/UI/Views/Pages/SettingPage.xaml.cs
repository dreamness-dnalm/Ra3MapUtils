using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views.Pages;

public partial class SettingPage : UserControl
{
    public SettingPage(SettingPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
