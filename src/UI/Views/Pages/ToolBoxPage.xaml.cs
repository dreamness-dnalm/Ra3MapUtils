using UI.ViewModels;

namespace UI.Views.Pages;

public partial class ToolBoxPage
{
    public ToolBoxPage(ToolBoxPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
