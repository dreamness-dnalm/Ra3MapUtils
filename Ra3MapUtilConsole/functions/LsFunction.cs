using System;
using System.ComponentModel;
using CommandLine;
using UtilLib.mapFileHelper;

namespace Ra3MapUtilConsole.functions
{
    public static class LsFunction
    {
        public static int DoAction(LsOptions opts)
        {
            try
            {
                MapFileHelper.Ls(opts.Path);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
    
    [Verb("ls", HelpText = "列出地图文件")]
    public class LsOptions
    {
        [Value(0, MetaName = "target Dir", Required = false, HelpText = "目标路径(默认为地图默认路径)"), DefaultValue(null)]
        public string Path { get; set; }
    }
}