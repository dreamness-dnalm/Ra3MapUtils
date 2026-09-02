namespace Ra3MapUtils.Models;

/// <summary>
/// Globals exposed to nano-program Main.cs (legacy-compatible).
/// </summary>
public class CSharpProgramGlobals
{
    public Dictionary<string, string> ArgumentDictionary { get; set; } = new();

    public static CSharpProgramGlobals Of(Dictionary<string, string>? arguments)
    {
        return new CSharpProgramGlobals
        {
            ArgumentDictionary = arguments ?? new Dictionary<string, string>(),
        };
    }
}
