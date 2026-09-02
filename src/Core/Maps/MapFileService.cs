using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace Core.Maps;

public sealed class MapFileService : IMapFileService
{
    private readonly IMapCatalogService _catalogService;

    public MapFileService(IMapCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public string GetDefaultMapsRoot() => _catalogService.GetDefaultMapsRoot();

    public MapOperationResult OpenInExplorer(string path, bool selectPath = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return MapOperationResult.Fail("路径为空。");
            }

            if (selectPath && File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{path}\"",
                    UseShellExecute = true,
                });
                return MapOperationResult.Ok(path);
            }

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return MapOperationResult.Fail($"路径不存在：{path}");
            }

            var target = Directory.Exists(path) ? path : Path.GetDirectoryName(path)!;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{target}\"",
                UseShellExecute = true,
            });
            return MapOperationResult.Ok(target);
        }
        catch (Exception ex)
        {
            return MapOperationResult.Fail($"打开资源管理器失败：{ex.Message}");
        }
    }

    public MapOperationResult Rename(string mapDirectoryPath, string newMapName)
    {
        var validate = ValidateSourceMap(mapDirectoryPath);
        if (validate is not null)
        {
            return validate;
        }

        var nameCheck = ValidateNewMapName(newMapName);
        if (nameCheck is not null)
        {
            return nameCheck;
        }

        var sourceName = MapPathRules.GetMapName(mapDirectoryPath);
        if (string.Equals(sourceName, newMapName, StringComparison.OrdinalIgnoreCase))
        {
            return MapOperationResult.Fail("新地图名不能与原地图名相同。");
        }

        var mapsRoot = GetDefaultMapsRoot();
        var destination = Path.Combine(mapsRoot, newMapName);
        if (Directory.Exists(destination))
        {
            return MapOperationResult.Fail($"目标目录已存在：{destination}");
        }

        try
        {
            var displayName = GetDisplayName(mapDirectoryPath);
            CopyMapDirectory(mapDirectoryPath, sourceName, destination, newMapName);
            Directory.Delete(mapDirectoryPath, true);

            if (displayName is not null)
            {
                SetDisplayNameInternal(destination, newMapName, displayName);
            }

            return MapOperationResult.Ok(destination);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return MapOperationResult.Fail($"重命名失败：{ex.Message}");
        }
    }

    public MapOperationResult Clone(string mapDirectoryPath, string newMapName, string? newDisplayName = null)
    {
        var validate = ValidateSourceMap(mapDirectoryPath);
        if (validate is not null)
        {
            return validate;
        }

        var nameCheck = ValidateNewMapName(newMapName);
        if (nameCheck is not null)
        {
            return nameCheck;
        }

        var sourceName = MapPathRules.GetMapName(mapDirectoryPath);
        if (string.Equals(sourceName, newMapName, StringComparison.OrdinalIgnoreCase))
        {
            return MapOperationResult.Fail("新地图名不能与原地图名相同。");
        }

        var mapsRoot = GetDefaultMapsRoot();
        var destination = Path.Combine(mapsRoot, newMapName);
        if (Directory.Exists(destination))
        {
            return MapOperationResult.Fail($"目标目录已存在：{destination}");
        }

        try
        {
            CopyMapDirectory(mapDirectoryPath, sourceName, destination, newMapName);

            var display = newDisplayName;
            if (display is null)
            {
                var oldDisplay = GetDisplayName(mapDirectoryPath);
                if (oldDisplay is not null)
                {
                    display = oldDisplay + "_COPY";
                }
            }

            if (display is not null)
            {
                SetDisplayNameInternal(destination, newMapName, display);
            }

            return MapOperationResult.Ok(destination);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return MapOperationResult.Fail($"另存为失败：{ex.Message}");
        }
    }

    public MapOperationResult CompressZip(string mapDirectoryPath)
    {
        var validate = ValidateSourceMap(mapDirectoryPath);
        if (validate is not null)
        {
            return validate;
        }

        var mapsRoot = GetDefaultMapsRoot();
        if (!Directory.Exists(mapsRoot))
        {
            return MapOperationResult.Fail($"地图根目录不存在：{mapsRoot}");
        }

        var mapName = MapPathRules.GetMapName(mapDirectoryPath);
        var zipPath = Path.Combine(mapsRoot, mapName + ".zip");
        if (File.Exists(zipPath))
        {
            return MapOperationResult.Fail($"压缩文件已存在：{zipPath}");
        }

        try
        {
            ZipFile.CreateFromDirectory(mapDirectoryPath, zipPath, CompressionLevel.Optimal, includeBaseDirectory: true);
            return MapOperationResult.Ok(zipPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return MapOperationResult.Fail($"压缩失败：{ex.Message}");
        }
    }

    public MapOperationResult Delete(string mapDirectoryPath)
    {
        var validate = ValidateSourceMap(mapDirectoryPath);
        if (validate is not null)
        {
            return validate;
        }

        try
        {
            Directory.Delete(mapDirectoryPath, true);
            return MapOperationResult.Ok();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return MapOperationResult.Fail($"删除失败：{ex.Message}");
        }
    }

    public MapBatchDeleteResult DeleteMany(IEnumerable<string> mapDirectoryPaths)
    {
        var success = 0;
        var failures = new List<string>();
        foreach (var path in mapDirectoryPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var result = Delete(path);
            if (result.Succeeded)
            {
                success++;
            }
            else
            {
                failures.Add($"{MapPathRules.GetMapName(path)}: {result.ErrorMessage}");
            }
        }

        return new MapBatchDeleteResult
        {
            SuccessCount = success,
            Failures = failures,
        };
    }

    public string? GetDisplayName(string mapDirectoryPath)
    {
        if (!MapPathRules.IsMap(mapDirectoryPath))
        {
            return null;
        }

        try
        {
            var mapName = MapPathRules.GetMapName(mapDirectoryPath);
            var dict = LoadMapStr(mapDirectoryPath);
            var key = "MAP:" + mapName;
            return dict.TryGetValue(key, out var value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    public MapOperationResult SetDisplayName(string mapDirectoryPath, string displayName)
    {
        var validate = ValidateSourceMap(mapDirectoryPath);
        if (validate is not null)
        {
            return validate;
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return MapOperationResult.Fail("显示名不能为空。");
        }

        try
        {
            var mapName = MapPathRules.GetMapName(mapDirectoryPath);
            SetDisplayNameInternal(mapDirectoryPath, mapName, displayName.Trim());
            return MapOperationResult.Ok();
        }
        catch (Exception ex)
        {
            return MapOperationResult.Fail($"写入 map.str 失败：{ex.Message}");
        }
    }

    private MapOperationResult? ValidateSourceMap(string mapDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(mapDirectoryPath) || !Directory.Exists(mapDirectoryPath))
        {
            return MapOperationResult.Fail("地图目录不存在。");
        }

        if (!MapPathRules.IsMap(mapDirectoryPath))
        {
            return MapOperationResult.Fail("目标不是有效地图目录。");
        }

        var mapsRoot = GetDefaultMapsRoot();
        if (!MapPathRules.IsUnderRoot(mapDirectoryPath, mapsRoot))
        {
            return MapOperationResult.Fail("只能操作地图根目录下的地图。");
        }

        return null;
    }

    private static MapOperationResult? ValidateNewMapName(string newMapName)
    {
        if (string.IsNullOrWhiteSpace(newMapName))
        {
            return MapOperationResult.Fail("地图名不能为空。");
        }

        var trimmed = newMapName.Trim();
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            trimmed.Contains('\\') ||
            trimmed.Contains('/'))
        {
            return MapOperationResult.Fail("地图名包含非法字符。");
        }

        return null;
    }

    private static void CopyMapDirectory(
        string sourceDir,
        string sourceMapName,
        string destinationDir,
        string destinationMapName)
    {
        Directory.CreateDirectory(destinationDir);
        foreach (var filePath in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName.StartsWith(sourceMapName, StringComparison.Ordinal))
            {
                fileName = destinationMapName + fileName.Substring(sourceMapName.Length);
            }

            File.Copy(filePath, Path.Combine(destinationDir, fileName), overwrite: true);
        }
    }

    private static Dictionary<string, string> LoadMapStr(string mapPath)
    {
        var mapStrPath = Path.Combine(mapPath, "map.str");
        var retDict = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(mapStrPath))
        {
            return retDict;
        }

        var lines = File.ReadAllLines(mapStrPath, Encoding.UTF8);
        var status = 0;
        var currKey = "";
        var currValue = "";

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line == "")
            {
                continue;
            }

            if (line.Equals("END", StringComparison.OrdinalIgnoreCase))
            {
                if (status != 2)
                {
                    throw new InvalidDataException($"map.str 格式错误（行 {i + 1}）");
                }

                retDict[currKey] = currValue;
                status = 0;
                currKey = "";
                currValue = "";
            }
            else if (line.Length >= 2 && line.StartsWith('"') && line.EndsWith('"'))
            {
                if (status != 1)
                {
                    throw new InvalidDataException($"map.str 格式错误（行 {i + 1}）");
                }

                currValue = line.Substring(1, line.Length - 2);
                status = 2;
            }
            else
            {
                if (status != 0)
                {
                    throw new InvalidDataException($"map.str 格式错误（行 {i + 1}）");
                }

                currKey = line;
                status = 1;
            }
        }

        if (status != 0)
        {
            throw new InvalidDataException("map.str 格式不完整。");
        }

        return retDict;
    }

    private static void SaveMapStr(string mapPath, Dictionary<string, string> dataDict)
    {
        var mapStrPath = Path.Combine(mapPath, "map.str");
        var sb = new StringBuilder();
        foreach (var pair in dataDict)
        {
            sb.Append(pair.Key).Append('\n');
            sb.Append('"').Append(pair.Value).Append('"').Append('\n');
            sb.Append("END\n\n");
        }

        File.WriteAllText(mapStrPath, sb.ToString(), Encoding.UTF8);
    }

    private static void SetDisplayNameInternal(string mapPath, string mapName, string displayName)
    {
        var dict = LoadMapStr(mapPath);
        foreach (var key in dict.Keys.Where(k => k.StartsWith("MAP:", StringComparison.Ordinal)).ToList())
        {
            dict.Remove(key);
        }

        dict["MAP:" + mapName] = displayName;
        SaveMapStr(mapPath, dict);
    }
}
