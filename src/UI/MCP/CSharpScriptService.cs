using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.ScriptExecutor;
using ModelContextProtocol.Server;

namespace UI.MCP;

[McpServerToolType]
public class CSharpScriptService
{
    [McpServerTool(Name = "run_ra3_csharp_script"), Description("执行CSharp脚本")]
    public static string RunRa3CSharpScript(string csharpScript, string workingDirectory = null)
    {
        // 先创建包含常用包的 ScriptOptions
        var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
        
        // 然后将宿主程序已加载的程序集添加进去
        scriptOptions = ScriptExecutor.WithLoadedAssemblies(
            scriptOptions,
            excludeSystemAssemblies: true,
            excludeDynamicAssemblies: true
        );
        
        var executor = new ScriptExecutor(scriptOptions);
        var result = executor.Execute(csharpScript, workingDirectory:workingDirectory);

        return result.ToJson();
    }
    
    
    [McpServerTool, Description("查询CSharp程序集的类结构")]
    public object GetLibStructure(string assemblyName)
    {
    if (string.IsNullOrWhiteSpace(assemblyName))
        return new { error = "libraryName is required" };

        Assembly asm;

        try
        {
            // 尝试加载程序集
            asm = Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            return new { error = $"Cannot load assembly: {assemblyName}", detail = ex.Message };
        }

        var classes = asm.GetTypes()
            .Where(t => t.IsPublic && t.IsClass)
            .Select(t => new
            {
                name = t.Name,
                fullName = t.FullName,
                @namespace = t.Namespace,
                methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Where(m => m.DeclaringType == t)
                            .Select(m => new
                            {
                                name = m.Name,
                                returnType = m.ReturnType.FullName,
                                parameters = m.GetParameters()
                                    .Select(p => new
                                    {
                                        paramName = p.Name,
                                        type = p.ParameterType.FullName
                                    })
                            })
                            .ToList(),
                properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Select(p => new
                            {
                                name = p.Name,
                                type = p.PropertyType.FullName
                            })
                            .ToList(),
                fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Select(f => new
                            {
                                name = f.Name,
                                type = f.FieldType.FullName
                            })
                            .ToList()
            })
            .ToList();

        return new
        {
            assembly = asm.GetName().Name,
            classes
        };
    }
    
    
    [McpServerTool, Description("获取CSharp环境已加载的程序集列表")]
    public List<string> GetLoadedAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyNames = assemblies.Select(a => a.GetName().Name).ToList();
        return assemblyNames;
    }
    
    private static string BuildMethodSignature(MethodInfo m)
    {
        string parameters = string.Join(", ",
            m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}")
        );

        return $"{m.ReturnType.Name} {m.Name}({parameters})";
    }

    
    [McpServerTool, Description("查询指定方法的完整签名（支持重载）")]
    public object GetMethodSignature(string assemblyNameOrPath, string className, string methodName)
    {
        if (string.IsNullOrWhiteSpace(assemblyNameOrPath))
            return new { error = "assemblyNameOrPath is required" };

        if (string.IsNullOrWhiteSpace(className))
            return new { error = "className is required" };

        if (string.IsNullOrWhiteSpace(methodName))
            return new { error = "methodName is required" };

        Assembly asm;

        // 允许 assemblyName 或 dll 路径
        try
        {
            if (File.Exists(assemblyNameOrPath))
                asm = Assembly.LoadFrom(assemblyNameOrPath);
            else
                asm = Assembly.Load(assemblyNameOrPath);
        }
        catch (Exception ex)
        {
            return new { error = $"Cannot load assembly: {assemblyNameOrPath}", detail = ex.Message };
        }

        // 查找类型
        var type = asm.GetTypes().FirstOrDefault(t => t.Name == className || t.FullName == className);
        if (type == null)
        {
            return new
            {
                error = $"Class '{className}' not found in assembly '{asm.GetName().Name}'"
            };
        }

        // 查找方法（包括重载）
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                          .Where(m => m.Name == methodName)
                          .ToList();
        XmlDocProvider.LoadForAssembly(asm);
        if (!methods.Any())
        {
            return new
            {
                error = $"Method '{methodName}' not found in class '{className}'"
            };
        }

        var signatures = methods.Select(m => new
        {
            name = m.Name,
            returnType = m.ReturnType.FullName,
            parameters = m.GetParameters().Select(p => new
            {
                name = p.Name,
                type = p.ParameterType.FullName,
                description = XmlDocProvider.GetParamDescriptions(m).GetValueOrDefault(p.Name, "")
            }),
            signature = BuildMethodSignature(m),
            summary = XmlDocProvider.GetSummary(m),
            returns = XmlDocProvider.GetReturnDescription(m)
        });

        return new
        {
            assembly = asm.GetName().Name,
            className = type.FullName,
            methodName,
            overloadCount = signatures.Count(),
            signatures
        };
    }

    [McpServerTool, Description("查询指定类型（class/struct/interface/enum）的详细信息")]
    public object GetTypeInfo(string assemblyNameOrPath, string className)
    {
        if (string.IsNullOrWhiteSpace(assemblyNameOrPath))
            return new { error = "assemblyNameOrPath is required" };
        if (string.IsNullOrWhiteSpace(className))
            return new { error = "className is required" };

        Assembly asm;

        try
        {
            if (File.Exists(assemblyNameOrPath))
                asm = Assembly.LoadFrom(assemblyNameOrPath);
            else
                asm = Assembly.Load(assemblyNameOrPath);
        }
        catch (Exception ex)
        {
            return new { error = $"Cannot load assembly: {assemblyNameOrPath}", detail = ex.Message };
        }

        // 自动加载 XMLDoc
        XmlDocProvider.LoadForAssembly(asm);

        // 查找类型
        var type = asm.GetTypes().FirstOrDefault(t => t.Name == className || t.FullName == className);
        if (type == null)
            return new { error = $"Type '{className}' not found in assembly '{asm.GetName().Name}'" };

        // 类型前缀
        string kind = type.IsEnum ? "enum"
                     : type.IsClass ? "class"
                     : type.IsValueType ? "struct"
                     : type.IsInterface ? "interface"
                     : "unknown";

        // 属性
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(p => new
            {
                name = p.Name,
                type = p.PropertyType.FullName,
                summary = XmlDocProvider.GetSummary(p)
            });

        // 字段
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(f => new
            {
                name = f.Name,
                type = f.FieldType.FullName,
                summary = XmlDocProvider.GetSummary(f)
            });

        // 方法
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.DeclaringType == type)
            .GroupBy(m => m.Name)
            .Select(g => new
            {
                name = g.Key,
                overloadCount = g.Count(),
                summary = XmlDocProvider.GetSummary(g.First()) // 取首个方法的 summary（重载共享）
            });

        return new
        {
            assembly = asm.GetName().Name,
            fullName = type.FullName,
            kind,
            summary = XmlDocProvider.GetSummary(type),
            properties,
            fields,
            methods
        };
    }

    [McpServerTool, Description("查询指定枚举的所有枚举值及注释")]
    public object GetEnumValues(string assemblyNameOrPath, string enumName)
{
    if (string.IsNullOrWhiteSpace(assemblyNameOrPath))
        return new { error = "assemblyNameOrPath is required" };
    if (string.IsNullOrWhiteSpace(enumName))
        return new { error = "enumName is required" };

    Assembly asm;

    try
    {
        if (File.Exists(assemblyNameOrPath))
            asm = Assembly.LoadFrom(assemblyNameOrPath);
        else
            asm = Assembly.Load(assemblyNameOrPath);
    }
    catch (Exception ex)
    {
        return new { error = $"Cannot load assembly: {assemblyNameOrPath}", detail = ex.Message };
    }

    // 自动加载 XMLDoc
    XmlDocProvider.LoadForAssembly(asm);

    // 查找枚举
    var type = asm.GetTypes().FirstOrDefault(t =>
        t.IsEnum && (t.Name == enumName || t.FullName == enumName)
    );

    if (type == null)
        return new { error = $"Enum '{enumName}' not found in assembly '{asm.GetName().Name}'" };

    var values = Enum.GetValues(type)
        .Cast<object>()
        .Select(v =>
        {
            string memberName = Enum.GetName(type, v);
            var memberInfo = type.GetMember(memberName).First();
            return new
            {
                name = memberName,
                intValue = Convert.ToInt32(v),
                summary = XmlDocProvider.GetSummary(memberInfo)
            };
        });

    return new
    {
        assembly = asm.GetName().Name,
        enumFullName = type.FullName,
        values
    };
}


public static class XmlDocProvider
{
    // 缓存结构：key = "M:Namespace.Class.Method", value = "summary text"
    private static readonly ConcurrentDictionary<string, string> _summaryCache = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _paramCache = new();
    private static readonly ConcurrentDictionary<string, string> _returnsCache = new();

    private static readonly HashSet<Assembly> _loadedAssemblies = new();

    /// <summary>
    /// 自动加载程序集的 XMLDoc 文件
    /// </summary>
    public static void LoadForAssembly(Assembly asm)
    {
        if (_loadedAssemblies.Contains(asm))
            return;

        _loadedAssemblies.Add(asm);

        string dllPath = asm.Location;
        string xmlPath = Path.ChangeExtension(dllPath, ".xml");

        if (!File.Exists(xmlPath))
            return;

        var doc = XDocument.Load(xmlPath);

        foreach (var member in doc.Descendants("member"))
        {
            var nameAttr = member.Attribute("name");
            if (nameAttr == null)
                continue;

            string key = nameAttr.Value; // 例如：M:Namespace.Class.Method

            // summary
            var summary = member.Descendants("summary").FirstOrDefault()?.Value.Trim();
            if (!string.IsNullOrEmpty(summary))
                _summaryCache[key] = summary;

            // returns
            var returns = member.Descendants("returns").FirstOrDefault()?.Value.Trim();
            if (!string.IsNullOrEmpty(returns))
                _returnsCache[key] = returns;

            // params
            var paramDict = new Dictionary<string, string>();
            foreach (var param in member.Descendants("param"))
            {
                string paramName = param.Attribute("name")?.Value ?? "";
                string paramText = param.Value.Trim();
                if (!string.IsNullOrEmpty(paramName))
                    paramDict[paramName] = paramText;
            }

            if (paramDict.Count > 0)
                _paramCache[key] = paramDict;
        }
    }

    /// <summary>
    /// 获取方法的 XML summary
    /// </summary>
    public static string GetSummary(MemberInfo member)
    {
        string key = BuildMemberKey(member);
        if (_summaryCache.TryGetValue(key, out string summary))
            return summary;
        return "";
    }

    public static string GetReturnDescription(MemberInfo member)
    {
        string key = BuildMemberKey(member);
        if (_returnsCache.TryGetValue(key, out string text))
            return text;
        return "";
    }

    public static Dictionary<string, string> GetParamDescriptions(MemberInfo member)
    {
        string key = BuildMemberKey(member);
        if (_paramCache.TryGetValue(key, out var dict))
            return dict;
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// 构建 XMLDoc key，例如：
    /// - M:Namespace.Class.Method
    /// - P:Namespace.Class.Property
    /// - F:Namespace.Class.Field
    /// - T:Namespace.Class
    /// </summary>
    private static string BuildMemberKey(MemberInfo member)
    {
        string prefix = member.MemberType switch
        {
            MemberTypes.Method => "M",
            MemberTypes.Property => "P",
            MemberTypes.Field => "F",
            MemberTypes.TypeInfo or MemberTypes.NestedType => "T",
            _ => "X"
        };

        return $"{prefix}:{member.DeclaringType.FullName}.{member.Name}";
    }
}

public static class AssemblyAutoLoader
{
    private static readonly HashSet<string> _loaded = new();

    /// <summary>
    /// 自动从指定目录加载所有 DLL，并自动加载对应的 XMLDoc。
    /// </summary>
    public static void LoadAllAssembliesFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        string[] dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(dllPath);

                // 避免重复加载
                if (_loaded.Contains(fileName))
                    continue;

                var asm = Assembly.LoadFrom(dllPath);

                _loaded.Add(fileName);

                // XMLDoc 自动加载
                string xmlPath = Path.ChangeExtension(dllPath, ".xml");
                if (File.Exists(xmlPath))
                {
                    XmlDocProvider.LoadForAssembly(asm);
                }
            }
            catch
            {
                // 忽略无法加载的 DLL（系统 DLL / native DLL 等）
            }
        }
    }
}
    
}
