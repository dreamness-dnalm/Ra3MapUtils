using CommandLine;

namespace Ra3MapUtilConsole.functions
{
    public class MvFunction
    {
        
    }
    
    [Verb("mv", HelpText = "移动/重命名地图文件")]
    class MoveOptions
    {
        [Value(0, MetaName = "source path", Required = true, HelpText = "地图路径(若只写名字则在默认路径寻找)")]
        public string Path1 { get; set; }

        [Value(1, MetaName = "target path", Required = true, HelpText = "目标地图路径(若只写名字则在默认路径保存)")]
        public string Path2 { get; set; }
    }
}