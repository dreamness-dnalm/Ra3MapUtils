using System;
using CommandLine;
using UtilLib.luaScriptLoader;
using UtilLib.mapFileHelper;

namespace Ra3MapUtilConsole.functions
{
    public class LoadLuaFunction
    {
        public static int DoAction(LoadLuaOptions opts)
        {
            try
            {
                Console.WriteLine($"load lua, config: {opts.ConfigPath}");
                LuaScriptLoaderHelper.LoadLuaScript(opts.ConfigPath);
                return 0;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
    
    [Verb("loadLua", HelpText = "为地图快速加载Lua脚本")]
    public class LoadLuaOptions
    {
        [Value(0, MetaName = "configPath", Required = true, HelpText = "配置文件路径")]
        public string ConfigPath { get; set; }
    }
}