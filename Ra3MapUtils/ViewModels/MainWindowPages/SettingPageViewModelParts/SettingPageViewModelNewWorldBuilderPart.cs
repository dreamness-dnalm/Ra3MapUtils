using System.Windows.Forms;
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
    
    [RelayCommand]
    private void PickNewWorldBuilderPath()
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
            }
            else
            {
                MessageBox.Show("非法的新地编路径");
            }
        }
    }

    public void OnLoadNewWorldBuilderPart()
    {
        _newWorldBuilderModel = new NewWorldBuilderModel();
        ObservableUtil.Subscribe(_newWorldBuilderModel, this);
        if (! NewWorldBuilderBusiness.IsNewWorldBuilderPathValid)
        {
            MessageBox.Show("请在\"设置\"->\"新地编联动\"中配置合法的新地编路径 ");
            return;
        }
        InstallNewWorldBuilderPluginNow();
        
    }

    public void InstallNewWorldBuilderPluginNow()
    {
        _newWorldBuilderPluginService.CheckAndInstallNewPluginsAvailable(NewWorldBuilderModel, SettingModel.NewWorldBuilderPath);
    }
}