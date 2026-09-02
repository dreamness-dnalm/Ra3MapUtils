using System.Windows;
using UI.ViewModels.Tools;

namespace UI.Views.Tools;

public partial class FastHashCalculatorWindow : Window
{
    public FastHashCalculatorWindow(FastHashCalculatorWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
