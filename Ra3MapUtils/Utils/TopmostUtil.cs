using System.Runtime.InteropServices;

namespace Ra3MapUtils.Utils;

public class TopmostUtil
{
    // 导入 Windows API
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // 常量定义
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // 窗口列表和委托
    private  delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private  static List<WindowInfo> _windowList = new List<WindowInfo>();

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
    }
    
    
    // 公共方法：根据关键字模糊匹配并置顶窗口
    public static void SetTopMostByKeyword(string keyword)
    {
        RefreshWindows();
        var targets = FindWindowsByKeyword(keyword);
        ModifyWindowsTopMost(targets, HWND_TOPMOST);
    }

    // 公共方法：根据关键字模糊匹配并取消置顶
    public static void UnsetTopMostByKeyword(string keyword)
    {
        RefreshWindows();
        var targets = FindWindowsByKeyword(keyword);
        ModifyWindowsTopMost(targets, HWND_NOTOPMOST);
    }

    // 私有方法：查找匹配关键字的窗口
    private static List<WindowInfo> FindWindowsByKeyword(string keyword)
    {
        return _windowList
            .Where(w => !string.IsNullOrEmpty(w.Title) &&
                        w.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();
    }

    // 私有方法：修改窗口置顶状态
    private static void ModifyWindowsTopMost(List<WindowInfo> windows, IntPtr topMostFlag)
    {
        foreach (var window in windows)
        {
            SetWindowPos(
                window.Handle,
                topMostFlag,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW
            );
        }
    }

    // 私有方法：刷新窗口列表
    private static void RefreshWindows()
    {
        _windowList.Clear();
        EnumWindows(EnumWindowCallback, IntPtr.Zero);
    }

    // 枚举窗口回调函数
    private static bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
    {
        const int maxChars = 256;
        var sb = new System.Text.StringBuilder(maxChars);
        if (GetWindowText(hWnd, sb, maxChars) > 0)
        {
            _windowList.Add(new WindowInfo { Handle = hWnd, Title = sb.ToString() });
        }
        return true;
    }
}