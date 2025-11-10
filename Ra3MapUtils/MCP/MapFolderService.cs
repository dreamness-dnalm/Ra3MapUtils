using System.ComponentModel;
using ModelContextProtocol.Server;
using UtilLib.mapFileHelper;

namespace Ra3MapUtils.MCP;

[McpServerToolType]
public class MapFolderService
{
    [McpServerTool, Description("获取所有的ra3地图的列表")]
    public static List<string> GetMapList()
    {
        return MapFileHelper.Ls(null);
    }
}