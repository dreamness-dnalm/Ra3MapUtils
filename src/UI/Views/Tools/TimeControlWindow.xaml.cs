using System.Windows;
using UI.ViewModels.Tools;

namespace UI.Views.Tools;

public partial class TimeControlWindow : Window
{
    public TimeControlWindow(TimeControlWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
