using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using Ra3MapUtils.Utils;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ToolBoxPageViewModel: ObservableObject
{
    [ObservableProperty] private bool _isRa3CoronaTopmost;

    partial void OnIsRa3CoronaTopmostChanged(bool value)
    {
        if (SecurityPrincipalUtil.IsRunningAsAdministrator)
        { ;
            var keyword = "红色警戒：日冕";
            if (value)
            {
                TopmostUtil.SetTopMostByKeyword(keyword);
            }
            else
            {
                TopmostUtil.UnsetTopMostByKeyword(keyword);
            }
            _isRa3CoronaTopmost = value;
        }
        else
        {
            MessageBox.Show("请以管理员身份运行程序");
        }
    }
    
    [ObservableProperty] private bool _isNewWorldBuilderTopmost;
    
    partial void OnIsNewWorldBuilderTopmostChanged(bool value)
    {
        if (SecurityPrincipalUtil.IsRunningAsAdministrator)
        {
            var keyword = " New World Builder v";
            if (value)
            {
                TopmostUtil.SetTopMostByKeyword(keyword);
            }
            else
            {
                TopmostUtil.UnsetTopMostByKeyword(keyword);
            }
            _isNewWorldBuilderTopmost = value;
        }
        else
        {
            MessageBox.Show("请以管理员身份运行程序");
        }
    }
}