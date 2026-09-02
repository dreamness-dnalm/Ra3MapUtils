namespace Core.Maps;

/// <summary>
/// Resolves map short names / directories / .map files without UtilCoreLib.
/// </summary>
public static class MapPathResolver
{
    public static (string MapDirectory, string MapName) Resolve(
        string mapPathOrName,
        string? mapsRoot = null)
    {
        if (string.IsNullOrWhiteSpace(mapPathOrName))
        {
            throw new ArgumentException("Map path cannot be empty.", nameof(mapPathOrName));
        }

        var root = string.IsNullOrWhiteSpace(mapsRoot)
            ? new MapCatalogService().GetDefaultMapsRoot()
            : mapsRoot.Trim();

        var normalized = mapPathOrName.Trim().Replace('/', Path.DirectorySeparatorChar);
        string mapDirectory;
        if (!normalized.Contains(Path.DirectorySeparatorChar)
            && !normalized.Contains(Path.AltDirectorySeparatorChar))
        {
            mapDirectory = Path.Combine(root, normalized);
        }
        else
        {
            mapDirectory = Path.GetFullPath(normalized);
            if (mapDirectory.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
            {
                mapDirectory = Path.GetDirectoryName(mapDirectory)
                               ?? throw new ArgumentException("Invalid map file path: " + mapPathOrName);
            }
        }

        mapDirectory = mapDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var mapName = Path.GetFileName(mapDirectory);
        return (mapDirectory, mapName);
    }

    public static bool IsMap(string mapPathOrName, string? mapsRoot = null)
    {
        try
        {
            var (dir, _) = Resolve(mapPathOrName, mapsRoot);
            return MapPathRules.IsMap(dir);
        }
        catch
        {
            return false;
        }
    }

    public static string GetMapFilePath(string mapPathOrName, string? mapsRoot = null)
    {
        var (dir, name) = Resolve(mapPathOrName, mapsRoot);
        return Path.Combine(dir, name + ".map");
    }

    /// <summary>
    /// Opens as (mapsParentDirectory, mapName) for Ra3MapFacade.Open(parent, name).
    /// </summary>
    public static (string MapsParentDirectory, string MapName) ResolveForFacadeOpen(
        string mapFileOrDirectoryOrName,
        string? mapsRoot = null)
    {
        var (mapDirectory, mapName) = Resolve(mapFileOrDirectoryOrName, mapsRoot);
        var parent = Directory.GetParent(mapDirectory)
                     ?? throw new DirectoryNotFoundException("Cannot resolve maps parent for: " + mapDirectory);
        return (parent.FullName, mapName);
    }
}
