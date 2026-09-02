using System.ComponentModel;
using Dreamness.RA3.Map.Lua.SyntaxChecker;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using UI.Models;
using UI.Services;

namespace UI.MCP;

[McpServerToolType]
public class LuaService
{
    [McpServerTool, Description("检查lua4脚本语法")]
    public static SyntaxCheckResult CheckLua4Syntax(string script)
    {
        return LuaSyntaxChecker.CheckSyntax(script).Result;
    }

    [McpServerTool, Description("导出指定RA3地图的Lua导入方案为json文件")]
    public static LuaImportSchemeOperationResult ExportLuaImportScheme(string map, string jsonPath)
    {
        var service = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILuaImportService>();
        return service.ExportMapLuaImportScheme(map, jsonPath);
    }

    [McpServerTool, Description("按json导入方案向指定RA3地图导入Lua")]
    public static LuaImportSchemeOperationResult ImportLuaBySchemeJson(string map, string jsonPath)
    {
        var service = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILuaImportService>();
        return service.ImportLuaBySchemeJson(map, jsonPath);
    }
}
