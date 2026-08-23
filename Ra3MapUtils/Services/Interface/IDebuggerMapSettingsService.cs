using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

/// <summary>
/// 读取、保存调试器地图参数并启动调试器。
/// </summary>
public interface IDebuggerMapSettingsService
{
    /// <summary>调试器配置文件路径。</summary>
    string SettingsFilePath { get; }

    /// <summary>读取当前地图参数。</summary>
    DebuggerMapSettingsModel Load();

    /// <summary>保存地图参数，同时保留配置文件中的其他参数。</summary>
    void Save(DebuggerMapSettingsModel settings);

    /// <summary>启动调试器，使已保存的参数生效。</summary>
    Task<bool> StartDebuggerAsync();
}
