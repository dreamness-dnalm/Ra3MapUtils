using System.Windows;
using UI.ViewModels.Tools;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace UI.Views.Tools;

public partial class ImageEncodingToolWindow : Window
{
    private readonly ImageEncodingToolWindowViewModel _viewModel;

    public ImageEncodingToolWindow(ImageEncodingToolWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Closed += (_, _) => _viewModel.ClosedCommand.Execute(null);
    }

    private void RootWindow_OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void RootWindow_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            _viewModel.LoadImageFromFile(files[0]);
        }

        e.Handled = true;
    }
}
