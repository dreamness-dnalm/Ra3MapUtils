using System.IO;
using System.Text;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Parser.Asset.Base;
using Dreamness.Ra3.Map.Parser.Core.Base;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Script;
using SharedFunctionLib.Business;
using SharedFunctionLib.Models;
using UtilLib.mapFileHelper;
using UtilLib.mapstrFileHelper;

namespace Ra3MapUtils.Utils;

public static class MapLuaImporterUtil
{

    public static void ImportLua(Ra3MapFacade ra3Map, List<SimpleLuaLibConfigModel> luaLibConfigs)
    {
        if (RedundancyStr == null)
        {
            RedundancyStr = "";
            var factor = LuaImporterBusiness.LuaRedundancyFactor;
            for (int i = 0; i < factor; i++)
            {
                RedundancyStr += "-- end of script, please ignore this line";
            }
        }
        
        var context = ra3Map.ra3Map.Context;
        var scriptList = ((PlayerScriptsList)context.AssetDict[AssetNameConst.PlayerScriptsList]).ScriptLists[0];
        Console.WriteLine("lua lib cnt: " + luaLibConfigs.Count);

        for (int i = 0; i < luaLibConfigs.Count; i++)
        {
            var libConfig = luaLibConfigs[i];
            var libFileModel = SimpleLibFileModel.Load(libConfig.LibPath);

            var scriptGroup = (ScriptGroup)(Translate(libFileModel, context).Item1);
            if (scriptGroup != null)
            {
                // remove previous
                scriptList.ScriptGroups.Where(i => i.Name == scriptGroup.Name).ToList()
                    .ForEach(i => scriptList.ScriptGroups.Remove(i));
                
                scriptList.Add(scriptGroup);
            }
        }
        scriptList.MarkModified();
        ra3Map.Save();
    }

    private static string RedundancyStr = null;

    public static void ImportLua(string mapFilePath)
    {
        var (retPath, mapName) = MapFileHelper.TranslateMapPath(Path.GetDirectoryName(mapFilePath));
        var parentPath = Directory.GetParent(retPath).FullName;
        var ra3Map = Ra3MapFacade.Open(parentPath, mapName);
        ImportLua(ra3Map, LuaImporterBusiness.LoadLuaLibConfigModels(mapFilePath));
    }

    public static void ImportLuaWithActiveConfig(string mapFilePath)
    {
        var (retPath, mapName) = MapFileHelper.TranslateMapPath(Path.GetDirectoryName(mapFilePath));
        var parentPath = Directory.GetParent(retPath).FullName;
        var ra3Map = Ra3MapFacade.Open(parentPath, mapName);
        ImportLua(ra3Map, LuaImporterBusiness.LoadLuaLibConfigModels());
    }


    private static SimpleLibFileModel.Tuple2 Translate(SimpleLibFileModel libFileModel, BaseContext context)
    {
        if (libFileModel.FileType == "lua")
        {
            var path = Path.Combine(libFileModel.LibPath, libFileModel.FileName);
            return new SimpleLibFileModel.Tuple2(
                OfLuaScript(
                    context,
                    libFileModel.FileName,
                    new List<string>() { File.ReadAllText(path, Encoding.GetEncoding("utf-8")) },
                    libFileModel.IsEnabled,
                    libFileModel.IsIncluded,
                    libFileModel.RunOnce,
                    libFileModel.IsEvaluateEachFrame ? -1 : libFileModel.EvaluationInterval, // TODO: check
                    libFileModel.ActiveInEasy,
                    libFileModel.ActiveInMedium,
                    libFileModel.ActiveInHard,
                    false
                ), libFileModel.OrderNum
            );
        }else if (libFileModel.FileType == "dir")
        {
            var childScripts = new List<SimpleLibFileModel.Tuple2>();
            var childScriptGroups = new List<SimpleLibFileModel.Tuple2>();

            foreach (var child in libFileModel.Children)
            {
                var t = Translate(child, context);
                if (t.Item1 != null)
                {
                    string childFileType = child.FileType;
                    if (childFileType == "lua")
                    {
                        childScripts.Add(t);
                    }
                    else if (childFileType == "dir")
                    {
                        childScriptGroups.Add(t);
                    }
                }
            }

            return new SimpleLibFileModel.Tuple2(
                OfScriptGroup(
                    context,
                    libFileModel.FileName,
                    childScripts.OrderBy(t => t.Item2).Select(t => (Script)t.Item1).ToList(),
                    childScriptGroups.OrderBy(t => t.Item2).Select(t => (ScriptGroup)t.Item1).ToList(),
                    libFileModel.IsEnabled,
                    libFileModel.IsIncluded,
                    false
                ), libFileModel.OrderNum);

        }

        return new SimpleLibFileModel.Tuple2(null, 99);
    }
    
    
    
    private static Script OfLuaScript(BaseContext context, string name,  List<string> luaContents, bool isEnable, bool isInclude, bool runOnce, int evaluationInterval, bool activeInEasy, bool activeInMedium, bool activeInHard, bool isSubroutine)
    {
        if (!isInclude)
        {
            return null;
        }
        var script = Script.Default(name, context);
        
        var orCondition = OrCondition.Empty(context);
        orCondition.Conditions.Add(ScriptConditionContent.Of("CONDITION_TRUE", new List<string>(), context));
        script.ScriptOrConditions.Add(orCondition);
    
        script.IsActive = isEnable;
        script.DeactivateUponSuccess = runOnce;
        if (evaluationInterval > 0)
        {
            script.EvaluationInterval = evaluationInterval;
        }
        script.ActiveInEasy = activeInEasy;
        script.ActiveInMedium = activeInMedium;
        script.ActiveInHard = activeInHard;
        script.IsSubroutine  = isSubroutine;
        
        foreach (var luaContent in luaContents)
        {
            var content = luaContent;
            if (!content.StartsWith("#!ra3luabridge"))
            {
                content = "#!ra3luabridge\r\n" + content;
            }

            content += RedundancyStr;
            script.ScriptActionOnTrue.Add(
                ScriptAction.Of(
                    "DEBUG_MESSAGE_BOX",
                    new List<string>() {content}, 
                    context)
                );
        }

        return script;
    }

    private static ScriptGroup OfScriptGroup(BaseContext context, string name, List<Script> subScripts,
        List<ScriptGroup> subScriptGroups, bool isEnable, bool isInclude, bool isSubroutine)
    {
        if (!isInclude)
        {
            return null;
        }

        var scriptGroup = ScriptGroup.Empty(name, isEnable, isSubroutine, context);
        foreach (var s in subScripts)
        {
            scriptGroup.Add(s);
        }

        foreach (var g in subScriptGroups)
        {
            scriptGroup.Add(g);
        }
        return scriptGroup;
    }
}