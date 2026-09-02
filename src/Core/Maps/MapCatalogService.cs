namespace Core.Maps;

/// <summary>
/// Map catalog with the same validity rule as legacy UtilCoreLib MapFileHelper.IsMap/Ls:
/// a directory named X is a map iff it contains X.map.
/// </summary>
public sealed class MapCatalogService : IMapCatalogService
{
    public string GetDefaultMapsRoot()
    {
        // Matches common RA3 / MapCoreLib PathUtil.RA3MapFolder location.
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Red Alert 3",
            "Maps");
    }

    public MapCatalogResult ListMaps(string? mapsRoot = null)
    {
        var root = string.IsNullOrWhiteSpace(mapsRoot)
            ? GetDefaultMapsRoot()
            : mapsRoot.Trim();

        if (!Directory.Exists(root))
        {
            return new MapCatalogResult
            {
                MapsRoot = root,
                Maps = Array.Empty<MapEntry>(),
                ErrorMessage = $"地图目录不存在：{root}",
            };
        }

        try
        {
            var maps = new List<MapEntry>();
            foreach (var dirPath in Directory.GetDirectories(root))
            {
                if (!MapPathRules.IsMap(dirPath))
                {
                    continue;
                }

                maps.Add(CreateEntry(dirPath));
            }

            maps.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return new MapCatalogResult
            {
                MapsRoot = root,
                Maps = maps,
                ErrorMessage = null,
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new MapCatalogResult
            {
                MapsRoot = root,
                Maps = Array.Empty<MapEntry>(),
                ErrorMessage = $"无法读取地图目录：{ex.Message}",
            };
        }
    }

    public MapPreview? GetPreview(string mapDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(mapDirectoryPath) || !Directory.Exists(mapDirectoryPath))
        {
            return null;
        }

        try
        {
            var mapName = Path.GetFileName(
                mapDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var files = Directory.GetFiles(mapDirectoryPath)
                .Select(Path.GetFileName)
                .Where(f => !string.IsNullOrEmpty(f))
                .Cast<string>()
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            long totalSize = 0;
            DateTime? lastWrite = null;
            foreach (var filePath in Directory.GetFiles(mapDirectoryPath))
            {
                var info = new FileInfo(filePath);
                totalSize += info.Length;
                if (lastWrite is null || info.LastWriteTimeUtc > lastWrite)
                {
                    lastWrite = info.LastWriteTimeUtc;
                }
            }

            var thumbnail = files
                .Where(f => f.StartsWith(mapName, StringComparison.OrdinalIgnoreCase) &&
                            f.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Contains("_art", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
                .Select(f => Path.Combine(mapDirectoryPath, f))
                .FirstOrDefault();

            return new MapPreview
            {
                Name = mapName,
                DirectoryPath = mapDirectoryPath,
                Files = files,
                ThumbnailPath = thumbnail,
                LastWriteTimeUtc = lastWrite,
                TotalSizeBytes = totalSize,
                SizeDisplay = FormatSize(totalSize),
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static MapEntry CreateEntry(string dirPath)
    {
        var mapName = Path.GetFileName(dirPath);
        var fileCount = 0;
        DateTime? lastWrite = null;

        try
        {
            foreach (var filePath in Directory.GetFiles(dirPath))
            {
                fileCount++;
                var write = File.GetLastWriteTimeUtc(filePath);
                if (lastWrite is null || write > lastWrite)
                {
                    lastWrite = write;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Keep partial entry.
        }

        var summaryParts = new List<string> { $"{fileCount} 个文件" };
        if (lastWrite is not null)
        {
            summaryParts.Add(lastWrite.Value.ToLocalTime().ToString("yyyy/MM/dd HH:mm"));
        }

        return new MapEntry(mapName, dirPath, fileCount, lastWrite, string.Join(" · ", summaryParts));
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        double kb = bytes / 1024.0;
        if (kb < 1024)
        {
            return $"{kb:0.#} KB";
        }

        double mb = kb / 1024.0;
        if (mb < 1024)
        {
            return $"{mb:0.#} MB";
        }

        return $"{mb / 1024.0:0.##} GB";
    }
}
