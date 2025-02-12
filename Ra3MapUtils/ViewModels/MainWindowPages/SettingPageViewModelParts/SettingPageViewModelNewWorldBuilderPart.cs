using System.Windows.Forms;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Impl;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Business;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject
{
    [ObservableProperty] private NewWorldBuilderModel _newWorldBuilderModel;
    
    private INewWorldBuilderPluginService _newWorldBuilderPluginService = App.Current.Services.GetRequiredService<INewWorldBuilderPluginService>();
    
    [ObservableProperty] private string _newWorldBuilderPathHint = "";
    
    [ObservableProperty] private Brush _newWorldBuilderPathHintColor;
    
    [ObservableProperty] private string _newWorldBuilderPluginHint = "";
    
    [ObservableProperty] private Brush _newWorldBuilderPluginHintColor;

    [ObservableProperty] private bool _newWorldBuilderPluginReInstallButtonEnable = true;
    
    [RelayCommand]
    private async void PickNewWorldBuilderPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择新地编路径",
            Filter = "新地编|WbLauncher.exe",
            InitialDirectory = @"C:\",
            Multiselect = false
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            var selectedPath = dialog.FileName;
            if (NewWorldBuilderBusiness.IsPathValidForNewWorldBuilderPath(selectedPath))
            {
                NewWorldBuilderBusiness.NewWorldBuilderPath = selectedPath;
                NewWorldBuilderPathHint = "已绑定";
                NewWorldBuilderPathHintColor = Brushes.Green;
                await InstallNewWorldBuilderPluginNow();
            }
            else
            {
                MessageBox.Show("非法的新地编路径");
            }
        }
    }
    
    partial void OnNewWorldBuilderModelChanged(NewWorldBuilderModel value)
    {
        if (value.IsPluginsInstalled)
        {
            NewWorldBuilderPluginHint = "已安装";
            NewWorldBuilderPluginHintColor = Brushes.Green;
            NewWorldBuilderPluginReInstallButtonEnable = false;
        }else if (value.IsPluginsInstalling)
        {
            NewWorldBuilderPluginHint = "正在安装";
            NewWorldBuilderPluginHintColor = Brushes.CornflowerBlue;
            NewWorldBuilderPluginReInstallButtonEnable = false;
        }else if (value.IsPluginsInstallError)
        {
            NewWorldBuilderPluginHint = "安装失败,请关闭新地编后重试";
            NewWorldBuilderPluginHintColor = Brushes.PaleVioletRed;
            NewWorldBuilderPluginReInstallButtonEnable = true;
        }
    }

    public void OnLoadNewWorldBuilderPart()
    {
        _newWorldBuilderModel = new NewWorldBuilderModel();
        ObservableUtil.Subscribe(_newWorldBuilderModel, this);
        if (! NewWorldBuilderBusiness.IsNewWorldBuilderPathValid)
        {
            NewWorldBuilderPathHint = "未绑定/无效的路径";
            NewWorldBuilderPathHintColor = Brushes.PaleVioletRed;
            NewWorldBuilderPluginHint = "安装失败";
            NewWorldBuilderPluginHintColor = Brushes.PaleVioletRed;
            NewWorldBuilderPluginReInstallButtonEnable = true;
            MessageBox.Show("请在\"设置\"->\"新地编联动\"中配置合法的新地编路径 ");
            return;
        }
        else
        {
            NewWorldBuilderPathHint = "已绑定";
            NewWorldBuilderPathHintColor = Brushes.Green;
            InstallNewWorldBuilderPluginNow();
        }
       
    }
    

    [RelayCommand]
    public async Task<bool> InstallNewWorldBuilderPluginNow()
    {
        if(NewWorldBuilderModel.IsPluginsInstalling)
        {
            return false;
        }
        
        try
        {
            await _newWorldBuilderPluginService.CheckAndInstallNewPluginsAvailable(NewWorldBuilderModel, NewWorldBuilderBusiness.NewWorldBuilderPath);
        }catch (Exception e)
        {
            var taskDialog = new Ookii.Dialogs.WinForms.TaskDialog();
            taskDialog.WindowTitle = "插件安装/更新失败";
            taskDialog.MainInstruction = "请关闭新地编后重试";
            taskDialog.Content = "错误详情:" + e.Message;
            taskDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("立即重试"));
            taskDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("取消"));
            
            if(taskDialog.ShowDialog().Text == "立即重试")
            {
                return await InstallNewWorldBuilderPluginNow();
            }
        }

        return false;
    }
    
    [RelayCommand]
    public void OpenNewWorldBuilderPluginFolder()
    {
        _newWorldBuilderPluginService.OpenPluginFolder(SettingModel.NewWorldBuilderPath);
    }
}