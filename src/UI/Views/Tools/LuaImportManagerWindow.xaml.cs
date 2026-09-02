using System.Windows;
using UI.ViewModels;

namespace UI.Views.Tools;

public partial class LuaImportManagerWindow : Window
{
    private readonly LuaImportManagerWindowViewModel _viewModel;

    public LuaImportManagerWindow(LuaImportManagerWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    public void Initialize(string mapName, string mapDirectoryPath)
    {
        _viewModel.Initialize(mapName, mapDirectoryPath);
    }

    private void LuaImportManagerWindow_OnClosed(object? sender, EventArgs e)
    {
        _viewModel.OnClosed();
    }
}
