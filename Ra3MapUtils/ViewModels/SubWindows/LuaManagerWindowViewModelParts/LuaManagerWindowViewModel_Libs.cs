using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.Utils;

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

        var model = new LuaLibConfigModel(_mapFilePath, "lib_" + (_luaLibConfigs.Count + 1), "", orderNum, true);
        _luaImportService.SaveMapLuaLibConfig(model);
        LuaLibConfigs.Add(model);
        SelectedLuaLibConfig = model;
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
        SelectedLuaLibConfig = null;
    }
    
    [RelayCommand]
    private void RenameLuaLibConfig()
    {
        if (_selectedLuaLibConfig == null)
        {
            return;
        }
       
        var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
        inputDialog.MainInstruction = "请输入新名字";
        inputDialog.Content = "请输入新名字";
        inputDialog.WindowTitle = "重命名";
        
        if (inputDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }
        if(inputDialog.Input == "")
        {
            MessageBox.Show("名字不能为空");
            return;
        }

        if (_luaLibConfigs.Where(i => i.ShowingName == inputDialog.Input).ToList().Count > 0)
        {
            MessageBox.Show("名字不能重复");
            return;
        }
        
        try
        {
            _selectedLuaLibConfig.Rename(inputDialog.Input);
            _selectedLuaLibConfig.ShowingName = inputDialog.Input;
        }
        catch (Exception e)
        {
            MessageBox.Show("重命名失败, 详细错误: " + e.Message);
        }
    }
    
    [RelayCommand]
    private void UpdateLuaLibConfig(LuaLibConfigModel model)
    {
        model.Upsert();
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
                SelectedLuaLibConfig.LibPath = dialog.SelectedPath;
                SelectedLuaLibConfig.Upsert();
                ReloadLibPreview();
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
    private void DoImport()
    {
        if(_luaLibConfigs.Count == 0)
        {
            return;
        }
        MapLuaImporterUtil.ImportLuaWithActiveConfig(_mapFilePath);
    }

    [RelayCommand]
    private void ExportLuaImportScheme()
    {
        if (string.IsNullOrWhiteSpace(_mapFilePath))
        {
            MessageBox.Show("未选择地图，无法导出Lua导入方案。");
            return;
        }

        var fileNamePrefix = string.IsNullOrWhiteSpace(_mapName)
            ? "lua_import_scheme"
            : string.Join("_", _mapName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        using var dialog = new SaveFileDialog
        {
            Title = "导出Lua导入方案",
            Filter = "JSON文件 (*.json)|*.json",
            DefaultExt = "json",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"{fileNamePrefix}_lua_import_scheme.json"
        };

        var mapDir = Path.GetDirectoryName(_mapFilePath);
        if (!string.IsNullOrWhiteSpace(mapDir) && Directory.Exists(mapDir))
        {
            dialog.InitialDirectory = mapDir;
        }

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            var result = _luaImportService.ExportMapLuaImportScheme(_mapFilePath, dialog.FileName);
            MessageBox.Show($"已导出 {result.ItemCount} 个Lua导入项到：\n{result.JsonPath}");
        }
        catch (Exception e)
        {
            MessageBox.Show("导出Lua导入方案失败, 详细错误: " + e.Message);
        }
    }

    // [RelayCommand]
    // private void ImportLua()
    // {
    //     if(SelectedLuaLibConfig != null && SelectedLuaLibConfig.LibPath != "")
    //     {
    //         var libFileModel = LibFileModel.Load(SelectedLuaLibConfig.LibPath);
    //         //todo xml/直接导入
    //         if (true)
    //         {
    //             var scriptGroupXElement = libFileModel.Export();
    //             var indexOfSelected = LuaLibConfigs.IndexOf(SelectedLuaLibConfig);
    //             var behindScriptGroupName = "";
    //             try
    //             {
    //                 if (indexOfSelected > 0)
    //                 {
    //                     var behindLibPath = LuaLibConfigs[indexOfSelected - 1].LibPath;
    //                     behindScriptGroupName = Path.GetFileName(behindLibPath);
    //                 }
    //             }
    //             catch (Exception e)
    //             {
    //             
    //             }
    //         
    //             _luaImportService.UpsertScriptGroup(MapName, scriptGroupXElement, behindScriptGroupName);
    //         }
    //         
    //     }
    // }
}
