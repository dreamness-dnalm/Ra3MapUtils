namespace Core.Maps;

/// <summary>
/// Shared map path rules (legacy MapFileHelper.IsMap semantics).
/// </summary>
public static class MapPathRules
{
    public static bool IsMap(string mapPath)
    {
        if (string.IsNullOrWhiteSpace(mapPath) || !Directory.Exists(mapPath))
        {
            return false;
        }

        var mapName = Path.GetFileName(
            mapPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrEmpty(mapName))
        {
            return false;
        }

        return File.Exists(Path.Combine(mapPath, mapName + ".map"));
    }

    public static string GetMapName(string mapPath)
    {
        return Path.GetFileName(
            mapPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    public static bool IsUnderRoot(string mapPath, string mapsRoot)
    {
        var fullMap = Path.GetFullPath(mapPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(mapsRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return fullMap.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
               || string.Equals(fullMap, fullRoot, StringComparison.OrdinalIgnoreCase);
    }
}
