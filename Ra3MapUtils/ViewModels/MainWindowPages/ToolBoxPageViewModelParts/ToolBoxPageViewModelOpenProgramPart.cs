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

        var path = Path.Combine(Path.GetDirectoryName(NewWorldBuilderBusiness.NewWorldBuilderPath), "game", "WuRa3GameDebug.exe");
        ProgramUtil.Run(path);
    }
    
    
    
}