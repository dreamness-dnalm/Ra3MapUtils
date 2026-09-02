using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.ViewModels;
using Point = System.Windows.Point;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace UI.Views.Pages;

public partial class NanoProgramsPage
{
    private Point _dragStartPoint;
    private bool _dragPending;

    public NanoProgramsPage(NanoProgramsPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void NanoProgramList_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragPending = false;

        // Only allow reorder drag from the dedicated handle.
        if (e.OriginalSource is not DependencyObject source
            || FindNamedAncestor(source, "DragHandle") is null)
        {
            return;
        }

        _dragStartPoint = e.GetPosition(null);
        _dragPending = true;
    }

    private void NanoProgramList_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragPending || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(position.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var listBoxItem = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (listBoxItem?.DataContext is not NanoProgramListItemViewModel item)
        {
            return;
        }

        _dragPending = false;
        DragDrop.DoDragDrop(listBoxItem, item, DragDropEffects.Move);
    }

    private void NanoProgramList_OnDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not NanoProgramsPageViewModel vm)
        {
            return;
        }

        if (e.Data.GetData(typeof(NanoProgramListItemViewModel)) is not NanoProgramListItemViewModel source)
        {
            return;
        }

        var targetItem = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (targetItem?.DataContext is NanoProgramListItemViewModel target)
        {
            vm.MoveItem(source, target);
        }
    }

    private static FrameworkElement? FindNamedAncestor(DependencyObject? current, string name)
    {
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.Name == name)
            {
                return fe;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
