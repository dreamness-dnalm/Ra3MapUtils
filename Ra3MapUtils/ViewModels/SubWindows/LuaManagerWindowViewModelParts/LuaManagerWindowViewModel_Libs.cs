using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.ViewModels;

public partial class LuaManagerWindowViewModel
{
        [ObservableProperty] private ObservableCollection<LuaLibConfigModel> _luaLibConfigs = new ObservableCollection<LuaLibConfigModel>();
    
    [ObservableProperty] private LuaLibConfigModel _selectedLuaLibConfig;
    
    partial void OnSelectedLuaLibConfigChanged(LuaLibConfigModel value)
    {
        ReloadLibPreview();
    }

    [RelayCommand]
    private void AddLuaLibConfig()
    {
        var orderNum = 1;
        if (_luaLibConfigs.Count > 0)
        {
            orderNum = _luaLibConfigs.Max(o => o.OrderNum) + 1;
        }

        var model = new LuaLibConfigModel(_mapName, "lib_" + (_luaLibConfigs.Count + 1), "", orderNum);
        _luaImportService.SaveMapLuaLibConfig(model);
        LuaLibConfigs.Add(model);
    }
    
    [RelayCommand]
    private void DeleteLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }

        var tmpModel = _selectedLuaLibConfig;
        _luaLibConfigs.Remove(tmpModel);
        tmpModel.Delete();
    }
    
    [RelayCommand]
    private void RenameLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }
        // todo: 弹窗输入新名字
        _selectedLuaLibConfig.Rename(_selectedLuaLibConfig.ShowingName);
    }
    [RelayCommand]
    private void ChangePathLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }

        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "请选择Lua库路径";
            dialog.ShowHiddenFiles = true;
            dialog.ShowNewFolderButton = false;
            if(_selectedLuaLibConfig.LibPath != "")
            {
                dialog.SelectedPath = _selectedLuaLibConfig.LibPath;
            }
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _selectedLuaLibConfig.LibPath = dialog.SelectedPath;
                _selectedLuaLibConfig.Upsert();
            }
        }
        
    }
    
    [RelayCommand]
    private void UpPositionLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }
        if(_luaLibConfigs.Count <= 1)
        {
            return;
        }
        
        var index = _luaLibConfigs.IndexOf(_selectedLuaLibConfig);
        if (index == 0)
        {
            return;
        }
        int tmpNum = _luaLibConfigs[index].OrderNum;
        _luaLibConfigs[index].OrderNum = _luaLibConfigs[index - 1].OrderNum;
        _luaLibConfigs[index - 1].OrderNum = tmpNum;
        
        _luaLibConfigs[index].Upsert();
        _luaLibConfigs[index - 1].Upsert();
        
        _luaLibConfigs.Move(index, index - 1);
    }
    
    [RelayCommand]
    private void DownPositionLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }
        if(_luaLibConfigs.Count <= 1)
        {
            return;
        }
        
        var index = _luaLibConfigs.IndexOf(_selectedLuaLibConfig);
        if (index == _luaLibConfigs.Count - 1)
        {
            return;
        }
        int tmpNum = _luaLibConfigs[index].OrderNum;
        _luaLibConfigs[index].OrderNum = _luaLibConfigs[index + 1].OrderNum;
        _luaLibConfigs[index + 1].OrderNum = tmpNum;
        
        _luaLibConfigs[index].Upsert();
        _luaLibConfigs[index + 1].Upsert();
        
        _luaLibConfigs.Move(index, index + 1);
    }

    [RelayCommand]
    private void ImportLua()
    {
        if(SelectedLuaLibConfig != null && SelectedLuaLibConfig.LibPath != "")
        {
            var scriptGroupXElement = LibFileModel.Load(SelectedLuaLibConfig.LibPath).Export();
            var indexOfSelected = LuaLibConfigs.IndexOf(SelectedLuaLibConfig);
            var behindScriptGroupName = "";
            try
            {
                if (indexOfSelected > 0)
                {
                    var behindLibPath = LuaLibConfigs[indexOfSelected - 1].LibPath;
                    behindScriptGroupName = Path.GetFileName(behindLibPath);
                }
            }
            catch (Exception e)
            {
                
            }
            
            _luaImportService.UpsertScriptGroup(MapName, scriptGroupXElement, behindScriptGroupName);
        }
    }
}