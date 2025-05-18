using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MapCoreLibMod.Core.Util;
using Ra3MapBridge;
using Ra3MapUtils.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels;

public partial class BorderManagerWindowViewModel: ObservableObject
{
    [ObservableProperty] private string _mapName = "";

    [ObservableProperty] private string _windowTitle = "";
    
    [ObservableProperty] private ObservableCollection<MapBorderModel> _borderModels = new ObservableCollection<MapBorderModel>();
    
    [ObservableProperty] private MapBorderModel? _selectedBorderModel;

    private Ra3MapWrap _ra3MapWrap;
    
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
            
            _ra3MapWrap = Ra3MapWrap.Open(PathUtil.RA3MapFolder, _mapName);

            var boarders = _ra3MapWrap.GetBorders();
            
            foreach (var border in boarders)
            {
                var newBorderModel = new MapBorderModel();
                _borderModels.Add(newBorderModel);
                
                newBorderModel.X1 = border.Corner1X;
                newBorderModel.Y1 = border.Corner1Y;
                newBorderModel.X2 = border.Corner2X;
                newBorderModel.Y2 = border.Corner2Y;
            }
        }catch (Exception e)
        {
            MessageBox.Show($"打开地图失败: {_mapName}, detail: {e.Message}");
        }
        
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        try
        {
            if (_ra3MapWrap == null)
            {
                return;
            }
            
            _ra3MapWrap.GetBorders().Clear();
            foreach (var borderModel in _borderModels)
            {
                _ra3MapWrap.AddBorder(borderModel.X1, borderModel.Y1, borderModel.X2, borderModel.Y2);
            }
            _ra3MapWrap.Save();
            
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
        _ra3MapWrap = null;
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