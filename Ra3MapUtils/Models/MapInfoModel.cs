using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using UtilLib.mapFileHelper;

namespace Ra3MapUtils.Models;

public partial class MapInfoModel: ObservableObject
{
    [ObservableProperty] private string _mapName;

    [ObservableProperty] private string _mapSize = "未知";

    [ObservableProperty] private string _playerCnt = "未知";

    
    partial void OnMapNameChanged(string value)
    {
        try
        {
            Trace.WriteLine("MapInfoModel.MapName Changed: " + value);
            
            var mapInfoJsonModel = MapInfoJsonHelper.ReadMapInfoJson(value);
            var mapWidth = mapInfoJsonModel.mapWidth;
            var mapHeight = mapInfoJsonModel.mapHeight;
            if (mapWidth > -1 && mapHeight > -1)
            {
                MapSize = $"{mapWidth}x{mapHeight}";
            }
            else
            {
                MapSize = "未知";
            }

            var playerCnt = mapInfoJsonModel.playerCnt;
            if(mapWidth > -1 && mapHeight > -1)
            {
                PlayerCnt = playerCnt.ToString();
            }
            else
            {
                PlayerCnt = "未知";
            }
        }
        catch (Exception e)
        {
            MapSize = "未知";
            PlayerCnt = "未知";
        }
        
    }
}