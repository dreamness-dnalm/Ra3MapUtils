using System.Windows;
using UI.ViewModels.Tools;

namespace UI.Views.Tools;

public partial class DebuggerMapSettingsWindow : Window
{
    public DebuggerMapSettingsWindow(DebuggerMapSettingsWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
