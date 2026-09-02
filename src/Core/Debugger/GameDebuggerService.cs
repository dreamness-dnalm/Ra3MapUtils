using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Core.Debugger;

public sealed class GameDebuggerService : IGameDebuggerService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

    public GameDebuggerService(string? baseDirectory = null)
    {
        var root = baseDirectory ?? AppContext.BaseDirectory;
        var debuggerDirectory = Path.Combine(root, "data", "Ra3Hacker");
        SettingsFilePath = Path.Combine(debuggerDirectory, "setting.json");
        InjectorExecutablePath = Path.Combine(debuggerDirectory, "Ra3Hacker.Injector.exe");
    }

    public string SettingsFilePath { get; }

    public string InjectorExecutablePath { get; }

    public bool IsInjectorPresent => File.Exists(InjectorExecutablePath);

    public DebuggerMapSettings LoadMapSettings()
    {
        var root = LoadRoot();
        var maps = root["maps"] as JsonObject;
        return new DebuggerMapSettings
        {
            MapFolder = maps?["folder"]?.GetValue<string>() ?? "",
            HideBuiltInMaps = maps?["hideBuiltIn"]?.GetValue<bool>() ?? false,
        };
    }

    public void SaveMapSettings(DebuggerMapSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var mapFolder = settings.MapFolder.Trim();
        if (mapFolder.Length > 0)
        {
            if (!Directory.Exists(mapFolder))
            {
                throw new DirectoryNotFoundException("指定的地图目录不存在：" + mapFolder);
            }

            mapFolder = Path.GetFullPath(mapFolder);
        }

        var root = LoadRoot();
        var maps = root["maps"] as JsonObject ?? new JsonObject();
        maps["folder"] = mapFolder;
        maps["hideBuiltIn"] = settings.HideBuiltInMaps;
        root["maps"] = maps;

        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = SettingsFilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(
                temporaryPath,
                root.ToJsonString(JsonOptions) + Environment.NewLine,
                Utf8WithoutBom);
            File.Move(temporaryPath, SettingsFilePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public bool TryLaunchInjector(out string? errorMessage)
    {
        if (!IsInjectorPresent)
        {
            errorMessage = "调试工具不存在：" + InjectorExecutablePath;
            return false;
        }

        var workingDirectory = Path.GetDirectoryName(InjectorExecutablePath);
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            errorMessage = "调试工具目录无效：" + InjectorExecutablePath;
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = InjectorExecutablePath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
            });
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = "启动调试工具失败：" + ex.Message;
            return false;
        }
    }

    private JsonObject LoadRoot()
    {
        if (!File.Exists(SettingsFilePath))
        {
            throw new FileNotFoundException("调试器配置文件不存在。", SettingsFilePath);
        }

        var root = JsonNode.Parse(File.ReadAllText(SettingsFilePath, Encoding.UTF8)) as JsonObject;
        return root ?? throw new InvalidDataException("调试器配置文件不是有效的 JSON 对象。");
    }
}
