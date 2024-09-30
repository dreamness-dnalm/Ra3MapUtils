using CommandLine;

namespace Ra3MapUtilConsole.functions
{
    public class LoadLuaFunction
    {
        
    }
    
    [Verb("loadLua", HelpText = "为地图快速加载Lua脚本")]
    class LoadLuaOptions
    {
        [Value(0, MetaName = "configPath", Required = true, HelpText = "配置文件路径")]
        public string ConfigPath { get; set; }
    }
}