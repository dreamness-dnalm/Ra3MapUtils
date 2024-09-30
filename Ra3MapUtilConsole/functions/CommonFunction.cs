using CommandLine;

namespace Ra3MapUtilConsole.functions
{
    public class CommonFunction
    {
        
    }
    
    
    class CommonOpts
    {
        [Option("version", HelpText = "版本信息")]
        public bool Version { get; set; } 
    }
}