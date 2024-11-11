using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;
using MapCoreLib.Util;
using UtilLib.mapFileHelper;
using UtilLib.utils;

namespace UtilCoreLib.mapScriptHelper;

public static class MapScriptHelper
{
    public static Script MakeScript(MapDataContext context, string name, List<string> luaContents, bool isEnable, bool isInclude, bool runOnce)
    {
        Logger.WriteLog("start make script.. name=" + name);
        if (!isInclude)
        {
            return null;
        }
        Logger.WriteLog("start make script");
        var script = new Script();
        script.Name = name;
        script.scriptOrConditions = MakeConditionTrue(context);
        script.ScriptActionOnTrue = new List<ScriptAction>();
        script.isActive = isEnable;
        script.DeactivateUponSuccess = runOnce;
        
        Logger.WriteLog("script create success!");
        
        foreach (var luaContent in luaContents)
        {
            var content = "#!ra3luabridge\n\r" + luaContent;
            script.ScriptActionOnTrue.Add(ScriptAction.of(context, "DEBUG_MESSAGE_BOX", new List<object>{(object)content}));
        }

        return script;
    }
    
    public static ScriptGroup MakeScriptGroup(MapDataContext context, string name, List<Script> subScripts, List<ScriptGroup> subScriptGroups, bool isEnable, bool isInclude)
    {
        if (!isInclude)
        {
            return null;
        }
        Logger.WriteLog("start make script group");
        var scriptGroup = new ScriptGroup
        {
            Name = name,
            IsActive = isEnable
        };
        Logger.WriteLog("script group create success!");
        foreach (var subScript in subScripts)
        {
            scriptGroup.scripts.Add(subScript);
        }

        foreach (var subScriptGroup in subScriptGroups)
        {
            scriptGroup.scriptGroups.Add(subScriptGroup);
        }

        return scriptGroup;
    }

    public static List<OrCondition> MakeConditionTrue(MapDataContext context)
    {
        return new List<OrCondition>()
        {
            new OrCondition()
            {
                conditions = new List<ScriptCondition>()
                {
                    ScriptCondition.of(context, "CONDITION_TRUE")
                }
            }
        };
    }

    public static void test()
    {
        var mapName = "NewMap17";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.parse();
        var context = ra3Map.getContext();
        
        // context.getAsset<PlayerScriptsList>("PlayerScriptsList").scriptLists[0]
        // var mapScriptListener = new MapScriptListener();
        // ra3Map.visit(mapScriptListener);
        //
        // var scriptList = mapScriptListener.Value;
        //
        // var script = MakeScript(context, "ttttt", new List<string>{"aaaa"}, true, true, true);
        //
        // var scriptGroup = MakeScriptGroup(context, "g1", new List<Script>{script}, new List<ScriptGroup>(), true, true);
        //
        // MapScriptOperator.AddScriptGroup(context, scriptList, scriptGroup, null);
        // ra3Map.doSaveMap(ra3Map.mapPath);
    }

    public static void test2(MapDataContext context)
    {
        var scriptList = context.getAsset<PlayerScriptsList>("PlayerScriptsList").scriptLists[0];
        var script = MakeScript(context, "ttttt", new List<string>{"aaaa"}, true, true, true);
        
        var scriptGroup = MakeScriptGroup(context, "g2", new List<Script>{script}, new List<ScriptGroup>(), true, true);
            
        MapScriptOperator.AddScriptGroup(context, scriptList, scriptGroup, null);
        
    }
}