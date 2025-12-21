namespace Ra3MapUtils.Models;

public class CSharpProgramGlobals
{
    public Dictionary<string, string> ArgumentDictionary { get; set; }
    
    public static CSharpProgramGlobals Of(Dictionary<string, string> arguments)
    {
        return new CSharpProgramGlobals
        {
            ArgumentDictionary = arguments
        };
    }
}