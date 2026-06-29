using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Utils;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class ImageEncodingToolWindow : FluentWindow
{
    private const int WmDropFiles = 0x0233;
    private const uint DragQueryAllFiles = 0xFFFFFFFF;
    private const uint MsgFilterAllow = 1;
    private const uint MsgFilterAdd = 1;
    private const uint WmCopyData = 0x004A;
    private const uint WmCopyGlobalData = 0x0049;

    private HwndSource? _hwndSource;
    private IntPtr _windowHandle;

    public ImageEncodingToolWindowViewModel _imageEncodingToolWindowViewModel
        => (ImageEncodingToolWindowViewModel)DataContext;

    public ImageEncodingToolWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<ImageEncodingToolWindowViewModel>();
        InitializeComponent();

        if (SecurityPrincipalUtil.IsRunningAsAdministrator)
        {
            // Elevated windows cannot receive Explorer's normal OLE drop from a non-elevated process.
            // Disable WPF drop registration so Explorer can fall back to WM_DROPFILES.
            AllowDrop = false;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        if (SecurityPrincipalUtil.IsRunningAsAdministrator)
        {
            EnableShellFileDrop();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        DisableShellFileDrop();
        base.OnClosed(e);
    }

    private void RootWindow_OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = HasFileDrop(e) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void RootWindow_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            _imageEncodingToolWindowViewModel.LoadImageFromFile(files[0]);
        }

        e.Handled = true;
    }

    private void EnableShellFileDrop()
    {
        _windowHandle = new WindowInteropHelper(this).Handle;
        if (_windowHandle == IntPtr.Zero)
        {
            return;
        }

        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(WndProc);

        RevokeOleDropTarget(_windowHandle);

        // Published builds may run elevated, where Explorer's normal OLE drop can be blocked by UIPI.
        AllowShellDropMessageForProcess(WmDropFiles);
        AllowShellDropMessageForProcess(WmCopyData);
        AllowShellDropMessageForProcess(WmCopyGlobalData);
        AllowShellDropMessage(_windowHandle, WmDropFiles);
        AllowShellDropMessage(_windowHandle, WmCopyData);
        AllowShellDropMessage(_windowHandle, WmCopyGlobalData);
        DragAcceptFiles(_windowHandle, true);
    }

    private void DisableShellFileDrop()
    {
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;

        if (_windowHandle != IntPtr.Zero)
        {
            DragAcceptFiles(_windowHandle, false);
            _windowHandle = IntPtr.Zero;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmDropFiles)
        {
            TryLoadShellDroppedFile(wParam);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void TryLoadShellDroppedFile(IntPtr dropHandle)
    {
        try
        {
            var fileCount = DragQueryFile(dropHandle, DragQueryAllFiles, null, 0);
            for (uint index = 0; index < fileCount; index++)
            {
                var pathLength = DragQueryFile(dropHandle, index, null, 0);
                if (pathLength == 0)
                {
                    continue;
                }

                var pathBuilder = new StringBuilder((int)pathLength + 1);
                if (DragQueryFile(dropHandle, index, pathBuilder, (uint)pathBuilder.Capacity) == 0)
                {
                    continue;
                }

                _imageEncodingToolWindowViewModel.LoadImageFromFile(pathBuilder.ToString());
                break;
            }
        }
        finally
        {
            DragFinish(dropHandle);
        }
    }

    private static bool HasFileDrop(DragEventArgs e)
    {
        return e.Data.GetDataPresent(DataFormats.FileDrop);
    }

    private static void AllowShellDropMessage(IntPtr hwnd, uint message)
    {
        try
        {
            var filter = new ChangeFilterStruct
            {
                CbSize = (uint)Marshal.SizeOf<ChangeFilterStruct>()
            };

            ChangeWindowMessageFilterEx(hwnd, message, MsgFilterAllow, ref filter);
        }
        catch
        {
            // The normal WPF drop path still works when the filter API is unavailable.
        }
    }

    private static void AllowShellDropMessageForProcess(uint message)
    {
        try
        {
            ChangeWindowMessageFilter(message, MsgFilterAdd);
        }
        catch
        {
            // Per-window filtering above is preferred and remains in effect when available.
        }
    }

    private static void RevokeOleDropTarget(IntPtr hwnd)
    {
        try
        {
            RevokeDragDrop(hwnd);
        }
        catch
        {
            // The window may not have an OLE drop target registered, which is fine.
        }
    }

    [DllImport("shell32.dll")]
    private static extern void DragAcceptFiles(IntPtr hwnd, bool accept);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder? fileName, uint fileNameLength);

    [DllImport("shell32.dll")]
    private static extern void DragFinish(IntPtr hDrop);

    [DllImport("ole32.dll")]
    private static extern int RevokeDragDrop(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ChangeWindowMessageFilter(uint message, uint action);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ChangeWindowMessageFilterEx(
        IntPtr hwnd,
        uint message,
        uint action,
        ref ChangeFilterStruct changeFilterStruct);

    [StructLayout(LayoutKind.Sequential)]
    private struct ChangeFilterStruct
    {
        public uint CbSize;
        public uint ExtStatus;
    }
}
