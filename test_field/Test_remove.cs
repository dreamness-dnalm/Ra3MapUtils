using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Scripts.ScriptFile;
using MapCoreLib.Core.Util;
using MapCoreLib.Util;
using UtilLib.mapFileHelper;

namespace test_field;

public class Test_remve
{
    public static void Main()
    {
        var mapName = "NewMap17";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.parse();
        var context = ra3Map.getContext();

        var mapScriptListener = new MapScriptListener();
        ra3Map.visit(mapScriptListener);

        var scriptList = mapScriptListener.Value;
        scriptList.scripts.Clear();
        ra3Map.doSaveMap(ra3Map.mapPath);
    }
}