
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Microsoft.Win32;
using Ra3MapUtils.Models;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject
{
    [ObservableProperty] private LuaLibBindingModel _luaLibBindingModel = new LuaLibBindingModel();

    [ObservableProperty] private string _luaLibBindingPathHint = "";

    [ObservableProperty] private Brush _luaLibBindingPathHintColor;

    public void OnLoadLuaLibBindingPart()
    {
        _luaLibBindingModel.Reload();
        OnLuaLibBindingModelChanged(_luaLibBindingModel);
        // ObservableUtil.Subscribe(_luaLibBindingModel, this);
    }
    
    [RelayCommand]
    private void PickLuaLibPath()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "选择Lua库安装路径(空目录)"
        };
        openFolderDialog.ShowDialog();
        var dir = openFolderDialog.FolderName;
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        if (! Directory.Exists(dir))
        {
            MessageBox.Show("目录不存在");
            return;
        }

        if (Directory.EnumerateFileSystemEntries(dir).Any())
        {
            MessageBox.Show("请指定一个空文件夹. 如果该文件夹已经安装了lua库, 可以先删除, 稍候会重新安装");
            return;
        }

        LuaLibBindingModel.LuaLibPath = dir;
        
        OnLuaLibBindingModelChanged(_luaLibBindingModel);
        MessageBox.Show("开始安装!");
    }
    
    partial void OnLuaLibBindingModelChanged(LuaLibBindingModel value)
    {
        if (string.IsNullOrEmpty(value.LuaLibPath))
        {
            LuaLibBindingPathHint = "未绑定";
            LuaLibBindingPathHintColor = Brushes.Red;
        } 
        else if (! Directory.Exists(value.LuaLibPath))
        {
            LuaLibBindingPathHint = "目标文件夹不存在";
            LuaLibBindingPathHintColor = Brushes.Red;
        }
        else
        {
            LuaLibBindingPathHint = "已绑定";
            LuaLibBindingPathHintColor = Brushes.Green;
        }
        
    }
}