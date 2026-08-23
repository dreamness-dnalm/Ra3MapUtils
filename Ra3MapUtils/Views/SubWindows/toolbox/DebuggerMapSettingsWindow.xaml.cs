using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class DebuggerMapSettingsWindow : FluentWindow
{
    /// <summary>
    /// 创建调试器地图配置窗口。
    /// </summary>
    public DebuggerMapSettingsWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<DebuggerMapSettingsWindowViewModel>();
        InitializeComponent();
    }
}
