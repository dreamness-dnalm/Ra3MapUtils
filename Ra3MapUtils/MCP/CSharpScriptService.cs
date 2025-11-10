using System.ComponentModel;
using Dreamness.ScriptExecutor;
using ModelContextProtocol.Server;
using UtilLib.mapFileHelper;

namespace Ra3MapUtils.MCP;

[McpServerToolType]
public class CSharpScriptService
{
    [McpServerTool, Description("通过运行CSharp脚本来操作ra3地图")]
    public static string RunRa3CSharpScript(string csharpScript)
    {
        var executor = new ScriptExecutor();
        var result = executor.Execute(csharpScript);

        return result.ToJson();
    }
}