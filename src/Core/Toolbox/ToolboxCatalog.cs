using Core.Debugger;



namespace Core.Toolbox;



public enum ToolboxCategory

{

    MapAndData = 0,

    Developer = 1,

    Other = 2,

}



public static class ToolboxCategoryLabels

{

    public static string GetDisplayName(ToolboxCategory category) => category switch

    {

        ToolboxCategory.MapAndData => "地图与数据",

        ToolboxCategory.Developer => "开发者",

        ToolboxCategory.Other => "其他",

        _ => category.ToString(),

    };

}



public sealed class ToolAvailability

{

    public bool IsAvailable { get; init; } = true;



    public string? UnavailableReason { get; init; }



    public static ToolAvailability Available { get; } = new() { IsAvailable = true };



    public static ToolAvailability Unavailable(string reason) =>

        new() { IsAvailable = false, UnavailableReason = reason };

}



public sealed class ToolEntry

{

    public required string Id { get; init; }



    public required string Title { get; init; }



    public required string Description { get; init; }



    public required ToolboxCategory Category { get; init; }



    /// <summary>Segoe Fluent Icons glyph used on the toolbox card.</summary>

    public string Glyph { get; init; } = "\uE90F";



    public Func<ToolAvailability> GetAvailability { get; init; } = static () => ToolAvailability.Available;

}



public interface IToolboxCatalog

{

    IReadOnlyList<ToolEntry> GetTools();

}



/// <summary>

/// Built-in static registry for v2 toolbox tools (no filesystem discovery).

/// </summary>

public sealed class StaticToolboxCatalog : IToolboxCatalog

{

    public const string FastHashId = "fast-hash";

    public const string ImageEncodingId = "image-encoding";

    public const string DeveloperHostingId = "developer-hosting";

    public const string OpenDebuggerId = "open-debugger";

    public const string DebuggerMapPathId = "debugger-map-path";

    public const string TimeControlId = "time-control";

    public const string LuaExecutorId = "lua-executor";



    private readonly IReadOnlyList<ToolEntry> _tools;



    public StaticToolboxCatalog(IGameDebuggerService debugger)

    {

        ArgumentNullException.ThrowIfNull(debugger);



        _tools =

        [

            new ToolEntry

            {

                Id = FastHashId,

                Title = "FastHash计算器",

                Description = "传说中的增援代码",

                Category = ToolboxCategory.MapAndData,

                Glyph = "\uE8EF",

            },

            new ToolEntry

            {

                Id = ImageEncodingId,

                Title = "图片编码工具",

                Description = "图片转 JPG/PNG/WEBP Base64 和 Lua 片段",

                Category = ToolboxCategory.MapAndData,

                Glyph = "\uE91B",

            },

            new ToolEntry

            {

                Id = OpenDebuggerId,

                Title = "打开调试工具",

                Description = "启动 Ra3Hacker Injector（游戏内调试）",

                Category = ToolboxCategory.Developer,

                Glyph = "\uE7BA",

                GetAvailability = () => debugger.IsInjectorPresent

                    ? ToolAvailability.Available

                    : ToolAvailability.Unavailable("调试工具不存在：" + debugger.InjectorExecutablePath),

            },

            new ToolEntry

            {

                Id = DebuggerMapPathId,

                Title = "重设游戏地图路径",

                Description = "配置调试器加载地图目录与官方地图可见性",

                Category = ToolboxCategory.Developer,

                Glyph = "\uE8B7",

            },

            new ToolEntry

            {

                Id = TimeControlId,

                Title = "时间操控",

                Description = "暂停、加速与跳帧（需调试器 API）",

                Category = ToolboxCategory.Developer,

                Glyph = "\uE916",

            },

            new ToolEntry

            {

                Id = LuaExecutorId,

                Title = "Lua 执行器",

                Description = "向运行中的游戏执行地图 Lua",

                Category = ToolboxCategory.Developer,

                Glyph = "\uE943",

            },

            new ToolEntry

            {

                Id = DeveloperHostingId,

                Title = "本地服务 / 开发者接入",

                Description = "MCP 与 HTTP API（Swagger）接入说明",

                Category = ToolboxCategory.Developer,

                Glyph = "\uE774",

            },

        ];

    }



    public IReadOnlyList<ToolEntry> GetTools() => _tools;

}

