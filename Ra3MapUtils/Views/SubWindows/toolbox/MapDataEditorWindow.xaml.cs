using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class MapDataEditorWindow : FluentWindow
{
    public global::Ra3MapUtils.ViewModels.toolbox.MapDataEditorWindowViewModel _mapDataEditorWindowViewModel
        => (global::Ra3MapUtils.ViewModels.toolbox.MapDataEditorWindowViewModel)DataContext;

    public MapDataEditorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<global::Ra3MapUtils.ViewModels.toolbox.MapDataEditorWindowViewModel>();
        InitializeComponent();
    }

    private void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TreeViewItem treeViewItem)
        {
            treeViewItem.IsSelected = true;
            treeViewItem.Focus();
        }
    }
}
