using System.ComponentModel;
using Dreamness.RA3.Map.Lua.SyntaxChecker;
using ModelContextProtocol.Server;

namespace Ra3MapUtils.MCP;

[McpServerToolType]
public class LuaService
{
    [McpServerTool, Description("检查lua4脚本语法")]
    public static SyntaxCheckResult CheckLua4Syntax(string script)
    {
        return LuaSyntaxChecker.CheckSyntax(script).Result;
        
    }
}