using System.IO;
using System.Text;
using Core.LuaImport;
using Core.Maps;
using Dreamness.Ra3.Map.Facade.Core;
using Newtonsoft.Json;
using Ra3MapUtils.Utils;
using UI.Models;

namespace UI.Services;

public sealed class LuaImportService : ILuaImportService
{
    private readonly ILuaLibConfigStore _configStore;
    private readonly IMapCatalogService _mapCatalog;

    public LuaImportService(ILuaLibConfigStore configStore, IMapCatalogService mapCatalog)
    {
        _configStore = configStore;
        _mapCatalog = mapCatalog;
    }

    public LuaImportSchemeOperationResult ExportMapLuaImportScheme(string map, string jsonPath)
    {
        var resolvedMap = ResolveMap(map);
        var resolvedJsonPath = ResolveJsonPath(jsonPath, mustExist: false);

        var configs = _configStore.Load(resolvedMap.ConfigKey).ToList();
        if (configs.Count == 0 && !string.Equals(map, resolvedMap.ConfigKey, StringComparison.OrdinalIgnoreCase))
        {
            configs = _configStore.Load(map).ToList();
        }

        var scheme = new LuaImportSchemeModel
        {
            Items = configs
                .Where(i => !string.IsNullOrWhiteSpace(i.LibPath))
                .OrderBy(i => i.OrderNum)
                .Select(i => new LuaImportSchemeItemModel
                {
                    Name = i.ShowingName ?? string.Empty,
                    Path = i.LibPath ?? string.Empty,
                    Enabled = i.IsEnabled > 0,
                })
                .ToList(),
        };

        var parent = Path.GetDirectoryName(resolvedJsonPath);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(
            resolvedJsonPath,
            JsonConvert.SerializeObject(scheme, Formatting.Indented),
            Encoding.UTF8);

        return new LuaImportSchemeOperationResult
        {
            Map = resolvedMap.MapName,
            MapFilePath = resolvedMap.MapFilePath,
            JsonPath = resolvedJsonPath,
            ItemCount = scheme.Items.Count,
            EnabledItemCount = scheme.Items.Count(i => i.Enabled),
        };
    }

    public LuaImportSchemeOperationResult ImportLuaBySchemeJson(string map, string jsonPath)
    {
        var resolvedMap = ResolveMap(map);
        var resolvedJsonPath = ResolveJsonPath(jsonPath, mustExist: true);
        var scheme = LoadScheme(resolvedJsonPath);

        var configs = scheme.Items
            .Select((item, index) => ToLuaLibConfig(resolvedMap.ConfigKey, item, index, resolvedJsonPath))
            .ToList();

        var enabledConfigs = configs
            .Where(i => i.IsEnabled > 0)
            .ToList();

        if (enabledConfigs.Count == 0)
        {
            throw new ArgumentException("导入方案中没有启用的 Lua 导入项。");
        }

        _configStore.ReplaceAll(resolvedMap.ConfigKey, configs);

        var ra3Map = Ra3MapFacade.Open(resolvedMap.MapFilePath);
        MapLuaImporterUtil.ImportLua(ra3Map, enabledConfigs);

        return new LuaImportSchemeOperationResult
        {
            Map = resolvedMap.MapName,
            MapFilePath = resolvedMap.MapFilePath,
            JsonPath = resolvedJsonPath,
            ItemCount = configs.Count,
            EnabledItemCount = enabledConfigs.Count,
        };
    }

    private static LuaImportSchemeModel LoadScheme(string jsonPath)
    {
        var scheme = JsonConvert.DeserializeObject<LuaImportSchemeModel>(File.ReadAllText(jsonPath, Encoding.UTF8));
        if (scheme is null)
        {
            throw new ArgumentException("导入方案 JSON 为空或格式不正确。");
        }

        if (scheme.SchemaVersion != 1)
        {
            throw new ArgumentException($"不支持的 Lua 导入方案版本：{scheme.SchemaVersion}");
        }

        if (!string.Equals(scheme.Type, LuaImportSchemeModel.SchemeType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("JSON 文件不是 Ra3MapUtils Lua 导入方案。");
        }

        scheme.Items ??= new List<LuaImportSchemeItemModel>();
        return scheme;
    }

    private static LuaLibConfigRecord ToLuaLibConfig(
        string mapConfigKey,
        LuaImportSchemeItemModel item,
        int index,
        string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new ArgumentException($"导入方案第 {index + 1} 项缺少 name。");
        }

        if (string.IsNullOrWhiteSpace(item.Path))
        {
            throw new ArgumentException($"导入方案第 {index + 1} 项缺少 path。");
        }

        var libPath = ResolveSchemeItemPath(item.Path, jsonPath);
        if (!Directory.Exists(libPath))
        {
            throw new ArgumentException($"Lua 导入项路径不存在或不是目录：{libPath}");
        }

        return new LuaLibConfigRecord
        {
            MapName = mapConfigKey,
            ShowingName = item.Name.Trim(),
            LibPath = libPath,
            OrderNum = index,
            IsEnabled = item.Enabled ? 1 : 0,
        };
    }

    private static string ResolveSchemeItemPath(string itemPath, string jsonPath)
    {
        if (Path.IsPathRooted(itemPath))
        {
            return Path.GetFullPath(itemPath);
        }

        var baseDir = Path.GetDirectoryName(jsonPath) ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(baseDir, itemPath));
    }

    private static string ResolveJsonPath(string jsonPath, bool mustExist)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            throw new ArgumentException("JSON 路径不能为空。");
        }

        var resolvedJsonPath = Path.GetFullPath(jsonPath.Trim());
        if (mustExist && !File.Exists(resolvedJsonPath))
        {
            throw new FileNotFoundException("JSON 文件不存在。", resolvedJsonPath);
        }

        return resolvedJsonPath;
    }

    private ResolvedLuaImportMap ResolveMap(string map)
    {
        if (string.IsNullOrWhiteSpace(map))
        {
            throw new ArgumentException("地图不能为空。");
        }

        var input = map.Trim();
        var mapsRoot = _mapCatalog.GetDefaultMapsRoot();
        if (input.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            var mapFilePath = Path.GetFullPath(input);
            if (!File.Exists(mapFilePath))
            {
                throw new FileNotFoundException("地图文件不存在。", mapFilePath);
            }

            return new ResolvedLuaImportMap(
                Path.GetFileNameWithoutExtension(mapFilePath),
                mapFilePath,
                mapFilePath);
        }

        var (mapDirPath, mapName) = MapPathResolver.Resolve(input, mapsRoot);
        if (!MapPathRules.IsMap(mapDirPath))
        {
            throw new ArgumentException("目标不是有效的 RA3 地图：" + input);
        }

        var resolvedMapFilePath = Path.GetFullPath(Path.Combine(mapDirPath, mapName + ".map"));
        return new ResolvedLuaImportMap(mapName, resolvedMapFilePath, resolvedMapFilePath);
    }

    private sealed record ResolvedLuaImportMap(string MapName, string MapFilePath, string ConfigKey);
}
