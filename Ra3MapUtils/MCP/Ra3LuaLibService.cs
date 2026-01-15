using System.ComponentModel;
using System.IO;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Ra3MapUtils.MCP;

/// <summary>
/// Ra3 Lua库MCP服务，提供Lua API查询功能
/// </summary>
[McpServerToolType]
public class Ra3LuaLibService
{
    private static readonly string MetadataPath = "data/ra3_lua_lib_metadata.json";
    private static LuaLibMetadata? _cachedMetadata;

    /// <summary>
    /// Ra3 Lua库 懒加载元数据
    /// </summary>
    private static LuaLibMetadata LoadMetadata()
    {
        if (_cachedMetadata != null)
            return _cachedMetadata;

        if (!File.Exists(MetadataPath))
            throw new FileNotFoundException($"Lua库元数据文件不存在: {MetadataPath}");

        var json = File.ReadAllText(MetadataPath);
        _cachedMetadata = JsonSerializer.Deserialize<LuaLibMetadata>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return _cachedMetadata!;
    }

    [McpServerTool, Description("获取RA3 Lua库的概览信息，包括版本、分类、模块数量等")]
    public static object GetLibraryOverview()
    {
        var metadata = LoadMetadata();

        return new
        {
            metadata = metadata.Metadata,
            categories = metadata.Categories?.Select(c => new
            {
                name = c.Key,
                description = c.Value.Description,
                fileCount = c.Value.FileCount,
                moduleCount = c.Value.Modules?.Count ?? 0
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 列出所有模块/文件，可按分类筛选")]
    public static object ListModules(string? category = null)
    {
        var metadata = LoadMetadata();

        IEnumerable<LuaModule> modules = metadata.Modules ?? Enumerable.Empty<LuaModule>();

        if (!string.IsNullOrEmpty(category))
        {
            modules = modules.Where(m =>
                m.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        return new
        {
            totalCount = modules.Count(),
            modules = modules.Select(m => new
            {
                name = m.Name,
                category = m.Category,
                filePath = m.FilePath,
                description = m.Description,
                functionCount = m.Functions?.Count ?? 0
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 获取指定模块的所有函数，包括签名和文档")]
    public static object GetModuleFunctions(string moduleName)
    {
        var metadata = LoadMetadata();

        // 先尝试精确匹配
        var module = metadata.Modules?.FirstOrDefault(m =>
            m.Name?.Equals(moduleName, StringComparison.OrdinalIgnoreCase) == true);

        if (module == null)
        {
            return new { error = $"模块 '{moduleName}' 不存在" };
        }

        return new
        {
            name = module.Name,
            category = module.Category,
            filePath = module.FilePath,
            description = module.Description,
            functionCount = module.Functions?.Count ?? 0,
            functions = module.Functions?.Select(f => new
            {
                name = f.Name,
                signature = f.Signature,
                description = f.Description,
                parameters = f.Parameters?.Select(p => new
                {
                    name = p.Name,
                    type = p.Type,
                    description = p.Description
                }),
                returns = f.Returns != null ? new
                {
                    type = f.Returns.Type,
                    description = f.Returns.Description
                } : null
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 搜索函数（精确匹配函数名）")]
    public static object SearchFunction(string functionName)
    {
        var metadata = LoadMetadata();

        // 使用搜索索引
        var key = functionName.ToLower();
        if (metadata.SearchIndex?.FunctionsByName?.TryGetValue(key, out var fullNames) == true)
        {
            var results = new List<object>();

            foreach (var fullName in fullNames)
            {
                // 解析 "ModuleName.FunctionName" 或 "ClassName:MethodName"
                if (fullName.Contains('.'))
                {
                    var parts = fullName.Split('.');
                    var moduleName = parts[0];
                    var funcName = parts[1];

                    var module = metadata.Modules?.FirstOrDefault(m => m.Name == moduleName);
                    var function = module?.Functions?.FirstOrDefault(f => f.Name == funcName);

                    if (function != null)
                    {
                        results.Add(new
                        {
                            type = "module_function",
                            module = moduleName,
                            category = module?.Category,
                            function = new
                            {
                                name = function.Name,
                                signature = function.Signature,
                                description = function.Description,
                                parameters = function.Parameters,
                                returns = function.Returns
                            }
                        });
                    }
                }
                else if (fullName.Contains(':'))
                {
                    var parts = fullName.Split(':');
                    var className = parts[0];
                    var methodName = parts[1];

                    var classObj = metadata.Classes?.FirstOrDefault(c => c.Name == className);
                    var method = classObj?.Methods?.FirstOrDefault(m => m.Name == methodName);

                    if (method != null)
                    {
                        results.Add(new
                        {
                            type = "class_method",
                            className = className,
                            category = classObj?.Category,
                            method = new
                            {
                                name = method.Name,
                                signature = method.Signature,
                                description = method.Description,
                                parameters = method.Parameters,
                                returns = method.Returns
                            }
                        });
                    }
                }
            }

            return new
            {
                query = functionName,
                matchCount = results.Count,
                results
            };
        }

        return new
        {
            query = functionName,
            matchCount = 0,
            results = Array.Empty<object>()
        };
    }

    [McpServerTool, Description("Ra3 Lua库 列出所有枚举类型")]
    public static object ListEnums()
    {
        var metadata = LoadMetadata();

        return new
        {
            totalCount = metadata.Enums?.Count ?? 0,
            enums = metadata.Enums?.Select(e => new
            {
                name = e.Name,
                filePath = e.FilePath,
                description = e.Description,
                valueCount = e.Values?.Count ?? 0
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 获取指定枚举的所有枚举值")]
    public static object GetEnumValues(string enumName)
    {
        var metadata = LoadMetadata();

        var enumObj = metadata.Enums?.FirstOrDefault(e =>
            e.Name?.Equals(enumName, StringComparison.OrdinalIgnoreCase) == true);

        if (enumObj == null)
        {
            return new { error = $"枚举 '{enumName}' 不存在" };
        }

        return new
        {
            name = enumObj.Name,
            filePath = enumObj.FilePath,
            description = enumObj.Description,
            valueCount = enumObj.Values?.Count ?? 0,
            values = enumObj.Values?.Select(v => new
            {
                key = v.Key,
                value = v.Value,
                description = v.Description
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 列出所有类")]
    public static object ListClasses()
    {
        var metadata = LoadMetadata();

        return new
        {
            totalCount = metadata.Classes?.Count ?? 0,
            classes = metadata.Classes?.Select(c => new
            {
                name = c.Name,
                filePath = c.FilePath,
                description = c.Description,
                fieldCount = c.Fields?.Count ?? 0,
                methodCount = c.Methods?.Count ?? 0
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 获取指定类的详细信息，包括字段和方法")]
    public static object GetClassInfo(string className)
    {
        var metadata = LoadMetadata();

        var classObj = metadata.Classes?.FirstOrDefault(c =>
            c.Name?.Equals(className, StringComparison.OrdinalIgnoreCase) == true);

        if (classObj == null)
        {
            return new { error = $"类 '{className}' 不存在" };
        }

        return new
        {
            name = classObj.Name,
            category = classObj.Category,
            filePath = classObj.FilePath,
            description = classObj.Description,
            fields = classObj.Fields?.Select(f => new
            {
                name = f.Name,
                type = f.Type,
                description = f.Description
            }),
            methods = classObj.Methods?.Select(m => new
            {
                name = m.Name,
                signature = m.Signature,
                description = m.Description,
                parameters = m.Parameters?.Select(p => new
                {
                    name = p.Name,
                    type = p.Type,
                    description = p.Description
                }),
                returns = m.Returns != null ? new
                {
                    type = m.Returns.Type,
                    description = m.Returns.Description
                } : null
            })
        };
    }

    [McpServerTool, Description("Ra3 Lua库 搜索模块（精确匹配模块名）")]
    public static object SearchModule(string moduleName)
    {
        var metadata = LoadMetadata();

        var key = moduleName.ToLower();
        if (metadata.SearchIndex?.ModulesByName?.TryGetValue(key, out var actualName) == true)
        {
            return GetModuleFunctions(actualName);
        }

        return new { error = $"模块 '{moduleName}' 不存在" };
    }
}

#region 数据模型类

/// <summary>
/// Ra3 Lua库元数据根对象
/// </summary>
public class LuaLibMetadata
{
    public MetadataInfo? Metadata { get; set; }
    public Dictionary<string, CategoryInfo>? Categories { get; set; }
    public List<LuaModule>? Modules { get; set; }
    public List<LuaEnum>? Enums { get; set; }
    public List<LuaClass>? Classes { get; set; }
    public SearchIndex? SearchIndex { get; set; }
}

/// <summary>
/// Ra3 Lua库 元信息
/// </summary>
public class MetadataInfo
{
    public string? Version { get; set; }
    public string? LastUpdate { get; set; }
    public string? SourceLibPath { get; set; }
    public int TotalModules { get; set; }
    public int TotalFunctions { get; set; }
}

/// <summary>
/// Ra3 Lua库 分类信息
/// </summary>
public class CategoryInfo
{
    public string? Description { get; set; }
    public int FileCount { get; set; }
    public List<string>? Modules { get; set; }
}

/// <summary>
/// Ra3 Lua库 Lua模块
/// </summary>
public class LuaModule
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? FilePath { get; set; }
    public string? Description { get; set; }
    public List<LuaFunction>? Functions { get; set; }
}

/// <summary>
/// Lua函数
/// </summary>
public class LuaFunction
{
    public string? Name { get; set; }
    public string? Signature { get; set; }
    public string? Description { get; set; }
    public List<LuaParameter>? Parameters { get; set; }
    public LuaReturn? Returns { get; set; }
}

/// <summary>
/// 函数参数
/// </summary>
public class LuaParameter
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// 函数返回值
/// </summary>
public class LuaReturn
{
    public string? Type { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Ra3 Lua库 Lua枚举
/// </summary>
public class LuaEnum
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? FilePath { get; set; }
    public string? Description { get; set; }
    public List<EnumValue>? Values { get; set; }
}

/// <summary>
/// Ra3 Lua库 枚举值
/// </summary>
public class EnumValue
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Ra3 Lua库 Lua类
/// </summary>
public class LuaClass
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? FilePath { get; set; }
    public string? Description { get; set; }
    public List<ClassField>? Fields { get; set; }
    public List<LuaFunction>? Methods { get; set; }
}

/// <summary>
/// Ra3 Lua库 类字段
/// </summary>
public class ClassField
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Ra3 Lua库 搜索索引
/// </summary>
public class SearchIndex
{
    public Dictionary<string, List<string>>? FunctionsByName { get; set; }
    public Dictionary<string, string>? ModulesByName { get; set; }
}

#endregion
