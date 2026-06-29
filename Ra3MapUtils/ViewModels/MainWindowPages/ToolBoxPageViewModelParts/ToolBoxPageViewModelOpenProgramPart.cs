using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3MapUtils.Utils;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ToolBoxPageViewModel: ObservableObject
{
    [RelayCommand]
    private void OpenNewWorldBuilderDebugProgram()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "Ra3Hacker", "Ra3Hacker.Injector.exe");
        if (!File.Exists(path))
        {
            MessageBox.Show("调试工具不存在: " + path);
            return;
        }

        var executableDirectory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(executableDirectory))
        {
            MessageBox.Show("调试工具目录无效: " + path);
            return;
        }

        _ = ProgramUtil.Run(path, executableDirectory);
    }
}
