using System.IO;
using System.Windows;
using Core.Maps;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UI;

namespace UI.MCP;

[McpServerToolType]
public class MapFileService
{
    [McpServerTool, Description("删除ra3地图")]
    public static void DeleteRa3Map(string mapName)
    {
        var services = ((App)Application.Current).Services;
        var catalog = services.GetRequiredService<IMapCatalogService>();
        var files = services.GetRequiredService<IMapFileService>();
        var (dir, _) = MapPathResolver.Resolve(mapName, catalog.GetDefaultMapsRoot());
        var result = files.Delete(dir);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Delete failed.");
        }
    }

    [McpServerTool, Description("复制ra3地图")]
    public static void CopyRa3Map(string sourceMapName, string targetMapName)
    {
        var services = ((App)Application.Current).Services;
        var catalog = services.GetRequiredService<IMapCatalogService>();
        var files = services.GetRequiredService<IMapFileService>();
        var root = catalog.GetDefaultMapsRoot();
        var (sourceDir, _) = MapPathResolver.Resolve(sourceMapName, root);
        var result = files.Clone(sourceDir, Path.GetFileName(targetMapName.TrimEnd('\\', '/')));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Copy failed.");
        }
    }

    [McpServerTool, Description("重命名ra3地图")]
    public static void RenameRa3Map(string sourceMapName, string targetMapName)
    {
        var services = ((App)Application.Current).Services;
        var catalog = services.GetRequiredService<IMapCatalogService>();
        var files = services.GetRequiredService<IMapFileService>();
        var root = catalog.GetDefaultMapsRoot();
        var (sourceDir, _) = MapPathResolver.Resolve(sourceMapName, root);
        var result = files.Rename(sourceDir, Path.GetFileName(targetMapName.TrimEnd('\\', '/')));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "Rename failed.");
        }
    }
}
