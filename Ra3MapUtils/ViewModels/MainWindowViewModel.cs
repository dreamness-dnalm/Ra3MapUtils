using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using MapCoreLib.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Utils;
using Ra3MapUtils.Views;
using UtilLib.mapFileHelper;
using MessageBox = System.Windows.MessageBox;

namespace Ra3MapUtils.ViewModels
{
    public partial class MainWindowViewModel: ObservableObject
    {
        [ObservableProperty] private ObservableCollection<string> _mapList = new();
        
        [ObservableProperty] private string _selectedMap = "";
        
        [ObservableProperty] private bool _compressMethodUseZip = true;
        
        private LuaManagerWindow _luaManagerWindow = App.Current.Services.GetRequiredService<LuaManagerWindow>();
        
        [RelayCommand]
        private void RefreshMapList()
        {
            try
            {
                var list = MapFileHelper.Ls(null);
                _mapList.Clear();
                foreach (var map in list)
                {
                    _mapList.Add(map);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("获取地图列表失败, msg: " + e.Message);
            }
        }
        
        [RelayCommand]
        private void DeleteMap()
        {
            try
            {
                if(_selectedMap == "")
                {
                    MessageBox.Show("请选择地图");
                    return;
                }
                
                var confirmDialog = new Ookii.Dialogs.WinForms.TaskDialog();
                confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("确认"));
                confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("取消"));
                confirmDialog.Content = "确认删除地图？";
                if(confirmDialog.ShowDialog().Text != "确认")
                {
                    return;
                }
                
                MapFileHelper.Del(_selectedMap);
                RefreshMapList();
            }
            catch(Exception e)
            {
                MessageBox.Show("删除失败, msg: " + e.Message);
            }
        }
        
        [RelayCommand]
        private void RenameMap()
        {
            try
            {
                if(_selectedMap == "")
                {
                    MessageBox.Show("请选择地图");
                    return;
                }
                
                var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
                inputDialog.MainInstruction = "请输入新地图名";
                inputDialog.Content = "请输入新地图名";
                inputDialog.WindowTitle = "重命名";
                inputDialog.Input = _selectedMap;

                if (inputDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                if(inputDialog.Input == "")
                {
                    MessageBox.Show("地图名不能为空");
                    return;
                }
                if(inputDialog.Input == _selectedMap)
                {
                    MessageBox.Show("新地图名不能与原地图名相同");
                    return;
                }
                
                MapFileHelper.Move(_selectedMap, inputDialog.Input);
                RefreshMapList();
            }
            catch(Exception e)
            {
                MessageBox.Show("重命名失败, msg: " + e.Message);
            }
        }
        
        [RelayCommand]
        private void CloneMap()
        {
            try
            {
                if(_selectedMap == "")
                {
                    MessageBox.Show("请选择地图");
                    return;
                }
                
                var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
                inputDialog.MainInstruction = "请输入新地图名";
                inputDialog.Content = "请输入新地图名";
                inputDialog.WindowTitle = "另存为";
                inputDialog.Input = _selectedMap;

                if (inputDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                if(inputDialog.Input == "")
                {
                    MessageBox.Show("地图名不能为空");
                    return;
                }
                if(inputDialog.Input == _selectedMap)
                {
                    MessageBox.Show("新地图名不能与原地图名相同");
                    return;
                }
                
                MapFileHelper.Copy(_selectedMap, inputDialog.Input);
                RefreshMapList();
            }
            catch(Exception e)
            {
                MessageBox.Show("另存为失败, msg: " + e.Message);
            }
        }
        
        [RelayCommand]
        private void OpenMapFolder()
        {
            // 在浏览器中打开地图文件夹, 并选中地图
            try
            {
                if(_selectedMap == "")
                {
                    ExplorerUtil.OpenExplorer(PathUtil.RA3MapFolder);
                    // Process.Start("explorer.exe", " " + PathUtil.RA3MapFolder);
                }
                else
                {
                    ExplorerUtil.OpenExplorer(Path.Combine(PathUtil.RA3MapFolder, _selectedMap), true);
                    // Process.Start("explorer.exe", "/select," + Path.Combine(PathUtil.RA3MapFolder, _selectedMap));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("打开地图文件夹失败, msg: " + e.Message);
            }
        }

        [RelayCommand]
        private void CompressMap()
        {
            try
            {
                if(_selectedMap == "")
                {
                    MessageBox.Show("请选择地图");
                    return;
                }
                
                var confirmDialog = new Ookii.Dialogs.WinForms.TaskDialog();
                confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("确认"));
                confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("取消"));
                confirmDialog.Content = "确认压缩地图?";
                if(confirmDialog.ShowDialog().Text != "确认")
                {
                    return;
                }

                var compressedFile = MapFileHelper.Compress(_selectedMap, PathUtil.RA3MapFolder, "zip");
                ExplorerUtil.OpenExplorer(compressedFile, true);
            }
            catch(Exception e)
            {
                MessageBox.Show("压缩失败, msg: " + e.Message);
            }
        }
        
        [RelayCommand]
        private void ManageLua()
        {
            if (_selectedMap == "")
            {
                MessageBox.Show("请选择地图");
                return;
            }
            
            var xmlFilePath = Path.Combine(PathUtil.RA3MapFolder, _selectedMap, _selectedMap + ".edit.xml");
            if (!File.Exists(xmlFilePath))
            {
                MessageBox.Show("xml不存在, 请先导出xml脚本: " + xmlFilePath);
                return;
            }

            _luaManagerWindow._luaManagerWindowViewModel.XmlFilePath = xmlFilePath;
            _luaManagerWindow.Show();
        }

    }
}