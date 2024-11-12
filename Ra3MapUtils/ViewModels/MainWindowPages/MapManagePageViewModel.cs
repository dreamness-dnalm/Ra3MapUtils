using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using MapCoreLib.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Utils;
using Ra3MapUtils.Views;
using UtilLib.mapFileHelper;
using TextBox = System.Windows.Controls.TextBox;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class MapManagePageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _mapList = new();

    [ObservableProperty] private string _selectedMap = "";

    [ObservableProperty] private MapInfoModel _selectedMapInfo = new();

    [ObservableProperty] private BitmapImage _thumbnail = new();

    [ObservableProperty] private ObservableCollection<string> _mapFileList = new();

    [ObservableProperty] private MapFileModel _selectedMapFile = new();

    private List<string> _actualMapFiles = new();

    private string _searchingKeyword = "";


    partial void OnSelectedMapChanged(string value)
    {
        if (value == "")
        {
            _selectedMapInfo = new MapInfoModel();
        }
        else
        {
            Trace.WriteLine("SelectedMapChanged: " + value);
            _selectedMapInfo.MapName = value;
            var (mapDir, _) = MapFileHelper.TranslateMapPath(value);
            SelectedMapFile = MapFileModel.Load(mapDir);

            _mapFileList.Clear();
            try
            {
                var mapFiles = MapFileHelper.LsMapFiles(value);
                mapFiles.ForEach(f => { _mapFileList.Add(f); });
            }
            catch (Exception e)
            {
                // MessageBox.Show("获取地图文件列表失败, msg: " + e.Message);
            }

            try
            {
                string picPath = _mapFileList
                    .Where(f => f.StartsWith(value) && f.EndsWith(".tga"))
                    .OrderBy(f => f.Contains("_art") ? 0 : 1)
                    .Take(1)
                    .SingleOrDefault("");

                if (picPath != "")
                {
                    picPath = Path.Combine(PathUtil.RA3MapFolder, value, picPath);
                }

                using (MagickImage magickImage = new MagickImage(picPath))
                {
                    byte[] imageBytes = magickImage.ToByteArray(MagickFormat.Bmp);

                    using (var ms = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        bitmapImage.Freeze();
                        Thumbnail = bitmapImage;
                    }
                }

            }
            catch (Exception e)
            {
                Thumbnail = new();
                // MessageBox.Show("获取地图信息失败, msg: " + e.Message);
            }

        }
    }


    [RelayCommand]
    private void RefreshMapList()
    {
        try
        {
            var list = MapFileHelper.Ls(null);
            _actualMapFiles.Clear();
            foreach (var map in list)
            {
                _actualMapFiles.Add(map);
            }

            Search();
        }
        catch (Exception e)
        {
            MessageBox.Show("获取地图列表失败, msg: " + e.Message);
        }
    }

    [RelayCommand]
    private void DeleteMap()
    {
        try
        {
            if (_selectedMap == "")
            {
                MessageBox.Show("请选择地图");
                return;
            }

            var confirmDialog = new Ookii.Dialogs.WinForms.TaskDialog();
            confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("确认"));
            confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("取消"));
            confirmDialog.Content = "确认删除地图？";
            if (confirmDialog.ShowDialog().Text != "确认")
            {
                return;
            }

            MapFileHelper.Del(_selectedMap);
            RefreshMapList();
        }
        catch (Exception e)
        {
            MessageBox.Show("删除失败, msg: " + e.Message);
        }
    }

    [RelayCommand]
    private void RenameMap()
    {
        try
        {
            if (_selectedMap == "")
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

            if (inputDialog.Input == "")
            {
                MessageBox.Show("地图名不能为空");
                return;
            }

            if (inputDialog.Input == _selectedMap)
            {
                MessageBox.Show("新地图名不能与原地图名相同");
                return;
            }

            MapFileHelper.Move(_selectedMap, inputDialog.Input);
            RefreshMapList();
        }
        catch (Exception e)
        {
            MessageBox.Show("重命名失败, msg: " + e.Message);
        }
    }

    [RelayCommand]
    private void CloneMap()
    {
        try
        {
            if (_selectedMap == "")
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

            if (inputDialog.Input == "")
            {
                MessageBox.Show("地图名不能为空");
                return;
            }

            if (inputDialog.Input == _selectedMap)
            {
                MessageBox.Show("新地图名不能与原地图名相同");
                return;
            }

            MapFileHelper.Copy(_selectedMap, inputDialog.Input);
            RefreshMapList();
        }
        catch (Exception e)
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
            if (_selectedMap == "")
            {
                ExplorerUtil.OpenExplorer(PathUtil.RA3MapFolder);
            }
            else
            {
                ExplorerUtil.OpenExplorer(Path.Combine(PathUtil.RA3MapFolder, _selectedMap), true);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show("打开地图文件夹失败, msg: " + e.Message);
        }
    }

    [RelayCommand]
    private void CompressMap()
    {
        try
        {
            if (_selectedMap == "")
            {
                MessageBox.Show("请选择地图");
                return;
            }

            var confirmDialog = new Ookii.Dialogs.WinForms.TaskDialog();
            confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("确认"));
            confirmDialog.Buttons.Add(new Ookii.Dialogs.WinForms.TaskDialogButton("取消"));
            confirmDialog.Content = "确认压缩地图?";
            if (confirmDialog.ShowDialog().Text != "确认")
            {
                return;
            }

            var compressedFile = MapFileHelper.Compress(_selectedMap, PathUtil.RA3MapFolder, "zip");
            ExplorerUtil.OpenExplorer(compressedFile, true);
        }
        catch (Exception e)
        {
            MessageBox.Show("压缩失败, msg: " + e.Message);
        }
    }

    [RelayCommand]
    private void ManageLua()
    {
        if (_selectedMap == "")
        {
            return;
        }

        if (GlobalVarsModel.LuaManagerWindowOpened)
        {
            MessageBox.Show("Lua导入工具已经打开");
            return;
        }

        var luaManagerWindow = App.Current.Services.GetRequiredService<LuaManagerWindow>();

        luaManagerWindow._luaManagerWindowViewModel.MapName = _selectedMap;
        luaManagerWindow.Show();
        GlobalVarsModel.SetLuaManagerWindowOpenedMapName(_selectedMap);
    }

    [RelayCommand]
    private void SearchTextChanged(TextChangedEventArgs arg)
    {
        _searchingKeyword = ((TextBox)arg.Source).Text;
        Search();
    }

    private void Search()
    {
        _mapList.Clear();
        _actualMapFiles.Where(i => _searchingKeyword == "" || i.Contains(_searchingKeyword))
            .ToList()
            .ForEach(i => _mapList.Add(i));
    }

    [RelayCommand]
    private void EditMapComment()
    {
        if (_selectedMap == "")
        {
            return;
        }
        var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
        inputDialog.MainInstruction = "请输入备注";
        inputDialog.Content = "请输入备注";
        inputDialog.WindowTitle = "备注";
        inputDialog.Input = SelectedMapFile.Comment;
        
        if (inputDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SelectedMapFile.Comment = inputDialog.Input;
            var (mapDir, _) = MapFileHelper.TranslateMapPath(SelectedMap);
            MapFileModel.SaveMeta(mapDir, SelectedMapFile);
        }
    }

}