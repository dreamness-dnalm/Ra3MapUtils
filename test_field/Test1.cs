using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Scripts.ScriptFile;
using MapCoreLib.Core.Util;
using MapCoreLib.Util;

namespace test_field;

public class Test1
{
    public static void Main()
    {
        var mapName = "NewMap17";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.parse();
        var context = ra3Map.getContext();
        
        Script script = new Script()
        {
            Name = "banInfantry2222",
            scriptOrConditions = new List<OrCondition>()
            {
                new OrCondition()
                {
                    conditions = new List<ScriptCondition>()
                    {
                        ScriptCondition.of(context, "CONDITION_TRUE")
                    }
                }
            }
        };
        
       script.ScriptActionOnTrue.Add(ScriptAction.of(context, "ALLOW_DISALLOW_ONE_BUILDING", new List<object>()
        {
            (object) "Player_1",
            (object) "building",
            (object) false
        }));
        Script.addTo(context, script, "/11/55");
        
        
        ra3Map.doSaveMap(ra3Map.mapPath);
    }
}