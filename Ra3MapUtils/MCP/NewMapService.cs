using System.ComponentModel;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.Util;
using ModelContextProtocol.Server;
using UtilLib.mapFileHelper;

namespace Ra3MapUtils.MCP;

[McpServerToolType]
public class NewMapService
{
    [McpServerTool, Description("新建一张ra3地图")]
    public static void CreateNewMap(string mapName, int playableWidth, int playableHeight, int border=0)
    {
        var ra3MapFacade = Ra3MapFacade.NewMap(playableWidth, playableHeight, border);
        ra3MapFacade.SaveAs(Ra3PathUtil.RA3MapFolder, mapName);
    }
}