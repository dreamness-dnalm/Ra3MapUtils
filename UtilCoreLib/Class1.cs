using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;
using UtilLib.mapFileHelper;

namespace UtilCoreLib;

public class Class1
{
    public static void main()
    {
        
        var mapName = "test_map5";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);
        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.parse();

        var context = ra3Map.getContext();
        
        var script = new Script()
        {
            Name = "banInfantry2",
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
        
        script.ScriptActionOnTrue.Add(
            ScriptAction.of(context, "ALLOW_DISALLOW_ONE_BUILDING", new List<object>(){"Player_1", "SovietAirfield", false}));

        Script.addTo(context,
            script,
            "");
        
        ra3Map.save(PathUtil.RA3MapFolder, "out_map");
    }
}