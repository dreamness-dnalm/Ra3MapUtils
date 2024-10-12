using CommandLine;

namespace Ra3MapUtilConsole.functions
{
    public class VersionFunction
    {
        public static int DoAction(VersionOptions opts)
        {
            System.Console.WriteLine(ProgramInfo.VersionInfo);
            return 0;
        }
    }
    
    [Verb("version", HelpText = "版本信息")]
    public class VersionOptions
    {
        
    }
}