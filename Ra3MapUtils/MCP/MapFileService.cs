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
    
}