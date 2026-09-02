using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace UI.Controls;

public partial class ToolWindowTitleBar : UserControl
{
    public ToolWindowTitleBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private Window? _hostWindow;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowChrome.SetIsHitTestVisibleInChrome(TopmostToggle, true);
        WindowChrome.SetIsHitTestVisibleInChrome(MinimizeButton, true);
        WindowChrome.SetIsHitTestVisibleInChrome(MaximizeButton, true);
        WindowChrome.SetIsHitTestVisibleInChrome(CloseButton, true);

        _hostWindow = Window.GetWindow(this);
        if (_hostWindow is null)
        {
            return;
        }

        _hostWindow.StateChanged += HostWindow_OnStateChanged;
        UpdateMaximizeGlyph();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_hostWindow is not null)
        {
            _hostWindow.StateChanged -= HostWindow_OnStateChanged;
            _hostWindow = null;
        }
    }

    private void HostWindow_OnStateChanged(object? sender, EventArgs e) => UpdateMaximizeGlyph();

    private void UpdateMaximizeGlyph()
    {
        if (_hostWindow?.WindowState == WindowState.Maximized)
        {
            MaximizeButton.Content = "\uE923"; // Restore
            MaximizeButton.ToolTip = TryFindResource("Chrome_Restore") as string ?? "还原";
        }
        else
        {
            MaximizeButton.Content = "\uE922"; // Maximize
            MaximizeButton.ToolTip = TryFindResource("Chrome_Maximize") as string ?? "最大化";
        }
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is { } window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not { } window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
