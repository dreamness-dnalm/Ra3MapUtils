using System.Reflection;
using System.IO;
using Core.NanoPrograms;
using Dreamness.ScriptExecutor;
using Ra3MapUtils.Models;
using Ra3MapUtils.ScriptViews;
using Ra3MapUtils.Utils;

namespace UI.Services;

public sealed class NanoProgramService : INanoProgramService
{
    private readonly NanoProgramCatalog _catalog;
    private readonly INanoProgramMetaStore _metaStore;
    private static int _hostAssembliesTouched;

    public NanoProgramService(NanoProgramCatalog catalog, INanoProgramMetaStore metaStore)
    {
        _catalog = catalog;
        _metaStore = metaStore;
        EnsureHostAssembliesLoaded();
    }

    public IReadOnlyList<NanoProgramModel> GetNanoPrograms() => _catalog.GetNanoPrograms();

    public void SaveMeta(string id, bool isEnabled, bool isWbVisible, int orderNum)
    {
        _metaStore.AddOrUpdate(id, isEnabled, isWbVisible, orderNum);
    }

    public NanoProgramExecutionResult ExecuteNanoProgram(string id, Dictionary<string, string>? arguments)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        EnsureHostAssembliesLoaded();

        var programs = _catalog.GetNanoPrograms();
        var record = programs.FirstOrDefault(p => string.Equals(p.Info.ID, id, StringComparison.OrdinalIgnoreCase));
        if (record == null)
        {
            throw new InvalidOperationException($"Nano program not found: {id}");
        }

        if (!record.IsEnabled)
        {
            throw new InvalidOperationException($"Nano program is disabled: {id}");
        }

        var csFilePath = Path.Combine(record.Info.Path, "Main.cs");
        if (!File.Exists(csFilePath))
        {
            throw new FileNotFoundException($"Nano program Main.cs not found: {csFilePath}", csFilePath);
        }

        var code = File.ReadAllText(csFilePath);
        var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
        scriptOptions = ScriptExecutor.WithLoadedAssemblies(
            scriptOptions,
            excludeSystemAssemblies: true,
            excludeDynamicAssemblies: true);
        var executor = new ScriptExecutor(scriptOptions);
        var globals = CSharpProgramGlobals.Of(arguments);
        var result = executor.Execute(
            code,
            workingDirectory: record.Info.Path,
            globals: globals,
            globalsType: typeof(CSharpProgramGlobals));

        return MapResult(result);
    }

    private static NanoProgramExecutionResult MapResult(ScriptExecutionResult result)
    {
        return new NanoProgramExecutionResult
        {
            Success = result.Success,
            Error = TryGetString(result, "Error", "Message"),
            ReturnValue = TryGetObject(result, "ReturnValue", "Result", "Value"),
            Output = TryGetString(result, "Output", "StdOut", "ConsoleOutput", "Logs"),
            Exception = TryGetExceptionString(result),
            ExceptionObject = TryGetExceptionObject(result),
        };
    }

    private static void EnsureHostAssembliesLoaded()
    {
        if (Interlocked.Exchange(ref _hostAssembliesTouched, 1) == 1)
        {
            return;
        }

        // Force-load script-facing helper types so WithLoadedAssemblies sees them.
        _ = typeof(MapFileSelectorDialog);
        _ = typeof(MsgDialog);
        _ = typeof(EasyDialog);
        _ = typeof(Ra3MapFacadeExtension);
        _ = typeof(MapLuaImporterUtil);
        _ = typeof(CSharpProgramGlobals);
        _ = typeof(Dreamness.Ra3.Map.Facade.Core.Ra3MapFacade);
    }

    private static string? TryGetString(object obj, params string[] propertyNames)
    {
        var value = TryGetObject(obj, propertyNames);
        return value switch
        {
            null => null,
            string s => s,
            _ => value.ToString(),
        };
    }

    private static object? TryGetObject(object obj, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                return prop.GetValue(obj);
            }
        }

        return null;
    }

    private static string? TryGetExceptionString(object obj)
    {
        return TryGetExceptionObject(obj)?.ToString();
    }

    private static Exception? TryGetExceptionObject(object obj)
    {
        var prop = obj.GetType().GetProperty("Exception", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return prop?.GetValue(obj) as Exception;
    }
}
