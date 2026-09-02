using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views.Pages;

public partial class MapManagePage
{
    public MapManagePage(MapManagePageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void MapList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MapManagePageViewModel vm)
        {
            return;
        }

        var items = MapList.SelectedItems.Cast<object>().ToList();
        vm.SyncSelectionFromList(items);
    }
}
