using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class NanoProgramPage : Page
{
    public NanoProgramPageViewModel _nanoProgramPageViewModel => (NanoProgramPageViewModel)DataContext;

    private Point _dragStartPoint;

    public NanoProgramPage()
    {
        DataContext = App.Current.Services.GetRequiredService<NanoProgramPageViewModel>();
        InitializeComponent();
    }

    private void NanoProgramList_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void NanoProgramList_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var listViewItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
        if (listViewItem?.DataContext is NanoProgramListItemViewModel item)
        {
            DragDrop.DoDragDrop(listViewItem, item, DragDropEffects.Move);
        }
    }

    private void NanoProgramList_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(NanoProgramListItemViewModel)) is not NanoProgramListItemViewModel source)
        {
            return;
        }

        var targetItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
        if (targetItem?.DataContext is NanoProgramListItemViewModel target)
        {
            _nanoProgramPageViewModel.MoveItem(source, target);
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
            {
                return target;
            }
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}