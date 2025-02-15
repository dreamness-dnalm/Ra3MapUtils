using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ToolBoxPageViewModel: ObservableObject
{
    private SettingPageViewModel _settingPageViewModel = App.Current.Services.GetRequiredService<SettingPageViewModel>();
    // private LogViewerWindow _logViewerWindow = App.Current.Services.GetRequiredService<LogViewerWindow>();
    
    [RelayCommand]
    private void OpenLogViewerWindow()
    {
        if ((!NewWorldBuilderBusiness.IsNewWorldBuilderPathValid) ||
            (!_settingPageViewModel.NewWorldBuilderModel.IsPluginsInstalled))
        {
            MessageBox.Show("请配置新地编联动, 在\"设置\"->\"新地编联动\"中设置");
            return;
        }
        
        // if (GlobalVarsModel.LogViewerWindowOpened)
        // {
        //     MessageBox.Show("日志查看工具已经打开");
        //     return;
        // }
        LogViewerWindow _logViewerWindow = App.Current.Services.GetRequiredService<LogViewerWindow>();
        _logViewerWindow._LogViewerWindowViewModel.OnLoad();
        _logViewerWindow.Show();

        GlobalVarsModel.LogViewerWindowOpened = true;
    }

    [RelayCommand]
    private void OpenMoreFunctionsWindow()
    {
        MessageBox.Show("欢迎加入QQ群: 513118543");
    }
}