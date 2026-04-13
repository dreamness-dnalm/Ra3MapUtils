using System.ComponentModel;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.Util;
using Dreamness.Ra3.Map.Parser.Util;
using Dreamness.ScriptExecutor;
using ModelContextProtocol.Server;
using UtilLib.mapFileHelper;

namespace Ra3MapUtils.MCP;

[McpServerToolType]
public class MapFileService
{
    [McpServerTool, Description("删除ra3地图")]
    public static void DeleteRa3Map(string mapName)
    {
        MapFileHelper.Del(mapName);
    }
    
    [McpServerTool, Description("复制ra3地图")]
    public static void CopyRa3Map(string sourceMapName, string targetMapName)
    {
        MapFileHelper.Copy(sourceMapName, targetMapName);
    }
    
    [McpServerTool, Description("重命名ra3地图")]
    public static void RenameRa3Map(string sourceMapName, string targetMapName)
    {
        MapFileHelper.Move(sourceMapName, targetMapName);
    }
    
    // [McpServerTool, Description("获取ra3地图尺寸")]
    // public static (int width, int height, int border, int playableWidth, int playableHeight) GetMapSize(string mapName)
    // {
    //     var ra3map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, mapName);
    //     
    //     return (ra3map.MapWidth, ra3map.MapHeight, ra3map.MapBorderWidth
    //         , ra3map.MapPlayableWidth, ra3map.MapPlayableHeight);
    // }
    //
    // [McpServerTool, Description("在ra3地图上放置路径点, 返回路径点名称")]
    // public static string PlaceWaypointsOnMap(string mapName, float x, float y, string name=null)
    // {
    //     var ra3map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, mapName);
    //     if (name == null)
    //     {
    //         var waypointWrap = ra3map.AddWaypoint(x, y);
    //         name = waypointWrap.WaypointName;
    //     }
    //     else
    //     {
    //         ra3map.AddWaypoint(name, x, y);
    //     }
    //     
    //     ra3map.Save();
    //     
    //     return name;
    // }
    //
    // [McpServerTool, Description("在ra3地图上移除路径点,物体")]
    // public static void RemoveWaypointsOnMap(string mapName, string name)
    // {
    //     var ra3map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, mapName);
    //     ra3map.Remove(name);
    //     ra3map.Save();
    // }
    //
    // [McpServerTool, Description("设置ra3路径点的位置")]
    // public static void SetWaypointPositionOnMap(string mapName, string name, float x, float y)
    // {
    //     var ra3map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, mapName);
    //     var waypointWrap = ra3map.GetWaypoints().First(w => w.WaypointName == name);
    //     waypointWrap.Position = new Vec3D(x, y, waypointWrap.Position.Z);
    //     ra3map.Save();
    // }
    
}