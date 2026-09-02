namespace Ra3MapUtils.ScriptViews;

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/// <summary>
/// 输入对话框类，自动处理置顶和初始化
/// </summary>
public abstract class BaseDialog
{
    // Windows API 声明
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_SHOWWINDOW = 0x0040;

    // 确保 Application 初始化的静态标志
    static bool appInitialized = false;

    // 安全的 Application 初始化方法
    static void EnsureApplicationInitialized()
    {
        if (!appInitialized)
        {
            try
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                appInitialized = true;
            }
            catch (InvalidOperationException)
            {
                // 如果已经初始化过，忽略异常
                appInitialized = true;
            }
        }
    }
    
    protected static Form _create(string title = "对话框", int width = 400, int height = 250, bool topMost = true)
    {
        EnsureApplicationInitialized();

        Form dialog = new Form();
        dialog.Text = title;
        dialog.Size = new Size(width, height);
        dialog.StartPosition = FormStartPosition.CenterScreen;
        dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
        dialog.MaximizeBox = false;
        dialog.MinimizeBox = false;
        dialog.TopMost = topMost;

        // 如果启用置顶，设置置顶功能
        if (topMost)
        {
            Action forceTopMost = () =>
            {
                try
                {
                    if (dialog.IsHandleCreated && !dialog.IsDisposed)
                    {
                        SetWindowPos(dialog.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    }
                }
                catch { }
            };

            // 在多个事件中设置置顶
            dialog.HandleCreated += (sender, e) => forceTopMost();
            dialog.Load += (sender, e) => forceTopMost();
            dialog.Shown += (sender, e) => forceTopMost();
            dialog.Activated += (sender, e) => forceTopMost();
            dialog.GotFocus += (sender, e) => forceTopMost();

            // // 使用定时器持续保持置顶
            // Timer topMostTimer = new Timer();
            // topMostTimer.Interval = 200;
            // topMostTimer.Tick += (sender, e) => forceTopMost();
            // topMostTimer.Start();
            //
            // // 当窗体关闭时停止定时器
            // dialog.FormClosed += (sender, e) =>
            // {
            //     topMostTimer.Stop();
            //     topMostTimer.Dispose();
            // };
        }

        return dialog;
    }

    /// <summary>
    /// 显示对话框并返回结果
    /// </summary>
    /// <param name="dialog">要显示的 Form 对象</param>
    /// <returns>返回 DialogResult</returns>
    public static DialogResult ShowDialog(Form dialog)
    {
        return dialog.ShowDialog();
    }
}
