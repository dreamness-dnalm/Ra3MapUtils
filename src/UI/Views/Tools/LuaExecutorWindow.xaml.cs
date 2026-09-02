using System.Windows;
using UI.ViewModels.Tools;

namespace UI.Views.Tools;

public partial class LuaExecutorWindow : Window
{
    public LuaExecutorWindow(LuaExecutorWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
