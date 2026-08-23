using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.Utils;

namespace Ra3MapUtils.Services.Impl;

/// <summary>
/// 管理 Ra3Hacker 的地图加载配置。
/// </summary>
public sealed class DebuggerMapSettingsService : IDebuggerMapSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
    private readonly string _debuggerExecutablePath;

    /// <summary>
    /// 使用应用程序目录中的 Ra3Hacker 配置创建服务。
    /// </summary>
    public DebuggerMapSettingsService()
        : this(AppContext.BaseDirectory)
    {
    }

    internal DebuggerMapSettingsService(string baseDirectory)
    {
        var debuggerDirectory = Path.Combine(baseDirectory, "data", "Ra3Hacker");
        SettingsFilePath = Path.Combine(debuggerDirectory, "setting.json");
        _debuggerExecutablePath = Path.Combine(debuggerDirectory, "Ra3Hacker.Injector.exe");
    }

    /// <inheritdoc />
    public string SettingsFilePath { get; }

    /// <inheritdoc />
    public DebuggerMapSettingsModel Load()
    {
        var root = LoadRoot();
        var maps = root["maps"] as JsonObject;

        return new DebuggerMapSettingsModel
        {
            MapFolder = maps?["folder"]?.GetValue<string>() ?? "",
            HideBuiltInMaps = maps?["hideBuiltIn"]?.GetValue<bool>() ?? false,
        };
    }

    /// <inheritdoc />
    public void Save(DebuggerMapSettingsModel settings)
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

    /// <inheritdoc />
    public Task<bool> StartDebuggerAsync()
    {
        if (!File.Exists(_debuggerExecutablePath))
        {
            throw new FileNotFoundException("调试工具不存在。", _debuggerExecutablePath);
        }

        return ProgramUtil.Run(_debuggerExecutablePath, Path.GetDirectoryName(_debuggerExecutablePath));
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
