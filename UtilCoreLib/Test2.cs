using MapCoreLib.Core.Scripts;
using MapCoreLib.Core.Util;

namespace UtilCoreLib;

public class Test2
{
    public static void Main()
    {
        var mapName = "test_map7";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);
        
        ScriptHandler.runScript(mapDir, "demo");
    }
}