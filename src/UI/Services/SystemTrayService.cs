using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UI.ViewModels;

namespace UI.Services;

/// <summary>
/// WinForms tray icon host. Marshal all UI actions onto the WPF dispatcher.
/// </summary>
public sealed class SystemTrayService : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly MainWindowViewModel _viewModel;
    private readonly ILocalizationService _localization;
    private NotifyIcon? _notifyIcon;
    private ToolStripMenuItem? _showItem;
    private ToolStripMenuItem? _mapsItem;
    private ToolStripMenuItem? _nanoItem;
    private ToolStripMenuItem? _toolboxItem;
    private ToolStripMenuItem? _settingsItem;
    private ToolStripMenuItem? _exitItem;
    private bool _disposed;

    public SystemTrayService(
        MainWindow mainWindow,
        MainWindowViewModel viewModel,
        ILocalizationService localization)
    {
        _mainWindow = mainWindow;
        _viewModel = viewModel;
        _localization = localization;
    }

    public void Start()
    {
        if (_notifyIcon is not null)
        {
            return;
        }

        var menu = new ContextMenuStrip();
        _showItem = new ToolStripMenuItem();
        _mapsItem = new ToolStripMenuItem();
        _nanoItem = new ToolStripMenuItem();
        _toolboxItem = new ToolStripMenuItem();
        _settingsItem = new ToolStripMenuItem();
        _exitItem = new ToolStripMenuItem();

        _showItem.Click += (_, _) => InvokeOnUi(_mainWindow.ShowFromTray);
        _mapsItem.Click += (_, _) => InvokeOnUi(() => NavigateAndShow("maps"));
        _nanoItem.Click += (_, _) => InvokeOnUi(() => NavigateAndShow("nano"));
        _toolboxItem.Click += (_, _) => InvokeOnUi(() => NavigateAndShow("toolbox"));
        _settingsItem.Click += (_, _) => InvokeOnUi(() => NavigateAndShow("settings"));
        _exitItem.Click += (_, _) => InvokeOnUi(RequestExit);

        menu.Items.Add(_showItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_mapsItem);
        menu.Items.Add(_nanoItem);
        menu.Items.Add(_toolboxItem);
        menu.Items.Add(_settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Visible = true,
            ContextMenuStrip = menu,
            Text = TruncateTip(_viewModel.Title),
        };
        _notifyIcon.DoubleClick += (_, _) => InvokeOnUi(_mainWindow.ShowFromTray);

        RefreshMenuLabels();
        _localization.LanguageChanged += OnLanguageChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _localization.LanguageChanged -= OnLanguageChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e) =>
        InvokeOnUi(RefreshMenuLabels);

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.Title) && _notifyIcon is not null)
        {
            InvokeOnUi(() =>
            {
                if (_notifyIcon is not null)
                {
                    _notifyIcon.Text = TruncateTip(_viewModel.Title);
                }
            });
        }
    }

    private void RefreshMenuLabels()
    {
        if (_showItem is null)
        {
            return;
        }

        _showItem.Text = _localization.GetString("Tray_Show", "显示主窗口");
        _mapsItem!.Text = _localization.GetString("Tray_Maps", "地图");
        _nanoItem!.Text = _localization.GetString("Tray_Nano", "微程序");
        _toolboxItem!.Text = _localization.GetString("Tray_Toolbox", "工具箱");
        _settingsItem!.Text = _localization.GetString("Tray_Settings", "设置");
        _exitItem!.Text = _localization.GetString("Tray_Exit", "退出");
        if (_notifyIcon is not null)
        {
            _notifyIcon.Text = TruncateTip(_viewModel.Title);
        }
    }

    private void NavigateAndShow(string key)
    {
        _viewModel.NavigateTo(key);
        _mainWindow.ShowFromTray();
    }

    private void RequestExit()
    {
        Dispose();
        _mainWindow.AllowClose();
        Application.Current?.Shutdown();
    }

    private static void InvokeOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            action();
            return;
        }

        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }

    private static Icon LoadTrayIcon()
    {
        var icoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
        if (File.Exists(icoPath))
        {
            return new Icon(icoPath);
        }

        return SystemIcons.Application;
    }

    private static string TruncateTip(string text)
    {
        // NotifyIcon.Text max length is 63 characters.
        if (string.IsNullOrEmpty(text))
        {
            return "Ra3MapUtils";
        }

        return text.Length <= 63 ? text : text[..63];
    }
}
