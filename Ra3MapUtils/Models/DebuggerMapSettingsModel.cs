namespace Ra3MapUtils.Models;

/// <summary>
/// 调试器读取的游戏地图加载配置。
/// </summary>
public sealed class DebuggerMapSettingsModel
{
    /// <summary>游戏加载地图的目录；空字符串表示使用游戏默认目录。</summary>
    public string MapFolder { get; init; } = "";

    /// <summary>是否隐藏游戏内置的官方地图。</summary>
    public bool HideBuiltInMaps { get; init; }
}
