using System;
using CommandLine;
using UtilLib.mapFileHelper;

namespace Ra3MapUtilConsole.functions
{
    public static class RemoveFunction
    {
        public static int DoAction(RemoveOptions opts)
        {
            try
            {
                Console.WriteLine($"Delete {opts.Path}");
                MapFileHelper.Del(opts.Path);
                return 0;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
    
    [Verb("mv", HelpText = "删除地图文件")]
    public class RemoveOptions
    {
        [Value(0, MetaName = "source path", Required = true, HelpText = "地图路径(若只写名字则在默认路径寻找)")]
        public string Path { get; set; }
    }
}