using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3MapUtils.Utils;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ToolBoxPageViewModel: ObservableObject
{
    [RelayCommand]
    private void OpenNewWorldBuilderDebugProgram()
    {
        if (!NewWorldBuilderBusiness.IsNewWorldBuilderPathValid)
        {
            MessageBox.Show("请先配置新地编路径");
            return;
        }

        var newWorldBuilderDirectory = Path.GetDirectoryName(NewWorldBuilderBusiness.NewWorldBuilderPath);
        if (string.IsNullOrWhiteSpace(newWorldBuilderDirectory))
        {
            MessageBox.Show("请先配置新地编路径");
            return;
        }

        var path = Path.Combine(newWorldBuilderDirectory, "game", "WuRa3GameDebug.exe");
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
