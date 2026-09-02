using System.Windows;
using Core.Maps;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UI;

namespace UI.MCP;

[McpServerToolType]
public class MapFolderService
{
    [McpServerTool, Description("获取所有的ra3地图的列表")]
    public static List<string> GetMapList()
    {
        var catalog = ((App)Application.Current).Services.GetRequiredService<IMapCatalogService>();
        var result = catalog.ListMaps();
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list maps.");
        }

        return result.Maps.Select(m => m.Name).ToList();
    }
}
