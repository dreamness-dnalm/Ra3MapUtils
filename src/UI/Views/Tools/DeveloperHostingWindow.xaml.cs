using System.Windows;
using UI.ViewModels.Tools;

namespace UI.Views.Tools;

public partial class DeveloperHostingWindow : Window
{
    public DeveloperHostingWindow(DeveloperHostingWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
