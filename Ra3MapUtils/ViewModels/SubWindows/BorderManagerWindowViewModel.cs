using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3MapFacade.Util;
using Ra3MapParser.Exception;
using Ra3MapUtils.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels;

public partial class BorderManagerWindowViewModel: ObservableObject
{
    [ObservableProperty] private string _mapName = "";

    [ObservableProperty] private string _windowTitle = "";
    
    [ObservableProperty] private ObservableCollection<MapBorderModel> _borderModels = new ObservableCollection<MapBorderModel>();
    
    [ObservableProperty] private MapBorderModel? _selectedBorderModel;

    [ObservableProperty] private int _minX;
    [ObservableProperty] private int _minY;
    [ObservableProperty] private int _maxX;
    [ObservableProperty] private int _maxY;

    private Ra3MapFacade.Ra3MapFacade _ra3MapFacade;
    
    partial void OnMapNameChanged(string value)
    {
        WindowTitle = $"边界管理工具 - {value}";
        _borderModels.Clear();

        if (value != null)
        {
            ReloadBoards();
        }
    }
    
    [RelayCommand]
    private void ReloadBoards()
    {
        try
        {
            _borderModels.Clear();

            if (_mapName == null || _mapName == "")
            {
                MessageBox.Show("请先选择地图");
                return;
            }

            _ra3MapFacade = Ra3MapFacade.Ra3MapFacade.Open(PathUtil.RA3MapFolder, _mapName);
            
            MinX = -_ra3MapFacade.MapBorderWidth;
            MinY = -_ra3MapFacade.MapBorderWidth;
            MaxX = _ra3MapFacade.MapPlayableWidth + _ra3MapFacade.MapBorderWidth;
            MaxY = _ra3MapFacade.MapPlayableHeight + _ra3MapFacade.MapBorderWidth;

            var boarders = _ra3MapFacade.GetBorders();

            foreach (var border in boarders)
            {
                var newBorderModel = new MapBorderModel();
                _borderModels.Add(newBorderModel);

                newBorderModel.X1 = border.X1;
                newBorderModel.Y1 = border.Y1;
                newBorderModel.X2 = border.X2;
                newBorderModel.Y2 = border.Y2;
            }
        }
        catch (BadMapException e)
        {
            _ra3MapFacade = null;
            MessageBox.Show($"打开地图失败: {_mapName}, 文件损坏或被加密.");
        }
        catch (Exception e)
        {
            MessageBox.Show($"打开地图失败: {_mapName}, detail: {e.Message}");
        }
        
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        try
        {
            if (_ra3MapFacade == null)
            {
                MessageBox.Show("尚未加载地图");
                return;
            }
            
            for(int i = 0; i < _borderModels.Count; i++)
            {
                var borderModel = _borderModels[i];
                if (borderModel.X1 < MinX || borderModel.Y1 < MinY || borderModel.X2 > MaxX || borderModel.Y2 > MaxY)
                {
                    MessageBox.Show($"第{i + 1}个边界非法[边界坐标超出范围: ({borderModel.X1}, {borderModel.Y1}) - ({borderModel.X2}, {borderModel.Y2})]");
                    return;
                }
                if(borderModel.X1 > borderModel.X2 || borderModel.Y1 > borderModel.Y2)
                {
                    MessageBox.Show($"第{i + 1}个边界非法[左下角坐标应大于右上角坐标: ({borderModel.X1}, {borderModel.Y1}) - ({borderModel.X2}, {borderModel.Y2})]");
                    return;
                }
            }

            _ra3MapFacade.GetBorders().Clear();
            foreach (var borderModel in _borderModels)
            {
                _ra3MapFacade.AddBorder(borderModel.X1, borderModel.Y1, borderModel.X2, borderModel.Y2);
            }
            _ra3MapFacade.Save();
            
            ReloadBoards();
        }catch (Exception e)
        {
            MessageBox.Show($"保存地图失败: {MapName}, detail: {e.Message}");
            return;
        }

    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.SetBorderManagerWindowOpenedMapName(null);
        GlobalVarsModel.BorderManagerWindowOpened = false;
        _ra3MapFacade = null;
        _minX = -1;
        _minY = -1;
        _maxX = -1;
        _maxY = -1;
    }

    [RelayCommand]
    private void AddBorder()
    {
        var mapBorderModel = new MapBorderModel();
        BorderModels.Add(mapBorderModel);
        mapBorderModel.X1 = 0;
        mapBorderModel.Y1 = 0;
        mapBorderModel.X2 = 10;
        mapBorderModel.Y2 = 10;
    }
    
    [RelayCommand]
    private void DeleteBorder()
    {
        if (SelectedBorderModel != null)
        {
            BorderModels.Remove(SelectedBorderModel);
        }

        SelectedBorderModel = null;
    }
    
    [RelayCommand]
    private void UpPositionBorder()
    {
        if (SelectedBorderModel == null)
        {
            return;
        }
        var tmp = SelectedBorderModel;
        var index = BorderModels.IndexOf(tmp);
        if (index == 0)
        {
            return;
        }

        BorderModels.RemoveAt(index);
        BorderModels.Insert(index - 1, tmp);
        CollectionViewSource.GetDefaultView(BorderModels)?.Refresh();
    }
    
    [RelayCommand]
    private void DownPositionBorder()
    {
        if (SelectedBorderModel == null)
        {
            return;
        }

        if (BorderModels.Count <= 1)
        {
            MessageBox.Show("无法删除, 至少需要一个边界");
            return;
        }
        var tmp = SelectedBorderModel;
        var index = BorderModels.IndexOf(tmp);
        if (index == BorderModels.Count - 1)
        {
            return;
        }

        BorderModels.RemoveAt(index);
        BorderModels.Insert(index + 1, tmp);
        CollectionViewSource.GetDefaultView(BorderModels)?.Refresh();
    }
    

    partial void OnSelectedBorderModelChanged(MapBorderModel value)
    {
        // throw new NotImplementedException();
    }
}