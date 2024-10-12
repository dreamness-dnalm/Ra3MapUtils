using System;
using CommandLine;
using UtilLib.mapFileHelper;

namespace Ra3MapUtilConsole.functions
{
    public static class CopyFunction
    {
        public static int DoAction(CopyOptions opts)
        {
            try
            {
                Console.WriteLine($"Copying from {opts.Path1} to {opts.Path2}");
                MapFileHelper.Copy(opts.Path1, opts.Path2);
                return 0;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
    
    [Verb("cp", HelpText = "复制地图文件")]
    public class CopyOptions
    {
        [Value(0, MetaName = "source path", Required = true, HelpText = "地图路径(若只写名字则在默认路径寻找)")]
        public string Path1 { get; set; }

        [Value(1, MetaName = "target path", Required = true, HelpText = "目标地图路径(若只写名字则在默认路径保存)")]
        public string Path2 { get; set; }
    }
}