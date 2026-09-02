using System.IO;
using System.Text;
using System.Windows;
using Core.LuaImport;
using Core.Maps;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Parser.Asset.Base;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Script;
using Dreamness.Ra3.Map.Parser.Core.Base;
using Microsoft.Extensions.DependencyInjection;
using UI;

namespace Ra3MapUtils.Utils;

public static class MapLuaImporterUtil
{
    private static string? _redundancyStr;
    private static int _cachedFactor = int.MinValue;

    public static void InvalidateRedundancyCache()
    {
        _redundancyStr = null;
        _cachedFactor = int.MinValue;
    }

    public static void ImportLua(Ra3MapFacade ra3Map, List<LuaLibConfigRecord> luaLibConfigs)
    {
        EnsureRedundancyStr();

        var context = ra3Map.ra3Map.Context;
        var scriptList = ((PlayerScriptsList)context.AssetDict[AssetNameConst.PlayerScriptsList]).ScriptLists[0];
        Console.WriteLine("lua lib cnt: " + luaLibConfigs.Count);

        for (var i = 0; i < luaLibConfigs.Count; i++)
        {
            var libConfig = luaLibConfigs[i];
            var libFileModel = LuaLibFileModel.Load(libConfig.LibPath);

            var scriptGroup = (ScriptGroup?)Translate(libFileModel, context).Item1;
            if (scriptGroup != null)
            {
                scriptList.ScriptGroups.Where(g => g.Name == scriptGroup.Name).ToList()
                    .ForEach(g => scriptList.ScriptGroups.Remove(g));

                scriptList.Add(scriptGroup);
            }
        }

        scriptList.MarkModified();
        ra3Map.Save();
    }

    public static void ImportLua(string mapFilePath)
    {
        var configs = LoadConfigs(mapFilePath);
        var (parent, mapName) = MapPathResolver.ResolveForFacadeOpen(mapFilePath);
        var ra3Map = Ra3MapFacade.Open(parent, mapName);
        ImportLua(ra3Map, configs);
    }

    public static void ImportLuaWithActiveConfig(string mapFilePath)
    {
        var settings = ResolveSettingsStore();
        var activeKey = settings.GetActiveMapName()
                        ?? throw new Exception(
                            "No Map Is Active In LuaImporter. Are you sure you have open the LuaImporter Window?");
        var configs = LoadConfigs(activeKey);
        var (parent, mapName) = MapPathResolver.ResolveForFacadeOpen(mapFilePath);
        var ra3Map = Ra3MapFacade.Open(parent, mapName);
        ImportLua(ra3Map, configs);
    }

    private static List<LuaLibConfigRecord> LoadConfigs(string mapKey)
    {
        var store = ResolveConfigStore();
        return store.Load(mapKey)
            .Where(i => !string.IsNullOrWhiteSpace(i.LibPath))
            .OrderBy(i => i.OrderNum)
            .ToList();
    }

    private static void EnsureRedundancyStr()
    {
        var factor = ResolveSettingsStore().GetRedundancyFactor();
        if (_redundancyStr is not null && _cachedFactor == factor)
        {
            return;
        }

        var sb = new StringBuilder(factor * 40);
        for (var i = 0; i < factor; i++)
        {
            sb.Append("-- end of script, please ignore this line");
        }

        _redundancyStr = sb.ToString();
        _cachedFactor = factor;
    }

    private static (object? Item1, int Item2) Translate(LuaLibFileModel libFileModel, BaseContext context)
    {
        if (libFileModel.FileType == "lua")
        {
            var path = Path.Combine(libFileModel.LibPath, libFileModel.FileName);
            return (
                OfLuaScript(
                    context,
                    libFileModel.FileName,
                    new List<string> { File.ReadAllText(path, Encoding.UTF8) },
                    libFileModel.IsEnabled,
                    libFileModel.IsIncluded,
                    libFileModel.RunOnce,
                    libFileModel.IsEvaluateEachFrame ? -1 : libFileModel.EvaluationInterval,
                    libFileModel.ActiveInEasy,
                    libFileModel.ActiveInMedium,
                    libFileModel.ActiveInHard,
                    false),
                libFileModel.OrderNum);
        }

        if (libFileModel.FileType == "dir")
        {
            var childScripts = new List<(Script Script, int Order)>();
            var childScriptGroups = new List<(ScriptGroup Group, int Order)>();

            foreach (var child in libFileModel.Children)
            {
                var t = Translate(child, context);
                if (t.Item1 is null)
                {
                    continue;
                }

                if (child.FileType == "lua")
                {
                    childScripts.Add(((Script)t.Item1, t.Item2));
                }
                else if (child.FileType == "dir")
                {
                    childScriptGroups.Add(((ScriptGroup)t.Item1, t.Item2));
                }
            }

            return (
                OfScriptGroup(
                    context,
                    libFileModel.FileName,
                    childScripts.OrderBy(t => t.Order).Select(t => t.Script).ToList(),
                    childScriptGroups.OrderBy(t => t.Order).Select(t => t.Group).ToList(),
                    libFileModel.IsEnabled,
                    libFileModel.IsIncluded,
                    false),
                libFileModel.OrderNum);
        }

        return (null, 99);
    }

    private static Script? OfLuaScript(
        BaseContext context,
        string name,
        List<string> luaContents,
        bool isEnable,
        bool isInclude,
        bool runOnce,
        int evaluationInterval,
        bool activeInEasy,
        bool activeInMedium,
        bool activeInHard,
        bool isSubroutine)
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
        script.IsSubroutine = isSubroutine;

        foreach (var luaContent in luaContents)
        {
            var content = luaContent;
            if (!content.StartsWith("#!ra3luabridge"))
            {
                content = "#!ra3luabridge\r\n" + content;
            }

            content += _redundancyStr;
            script.ScriptActionOnTrue.Add(
                ScriptAction.Of(
                    "DEBUG_MESSAGE_BOX",
                    new List<string> { content },
                    context));
        }

        return script;
    }

    private static ScriptGroup? OfScriptGroup(
        BaseContext context,
        string name,
        List<Script> subScripts,
        List<ScriptGroup> subScriptGroups,
        bool isEnable,
        bool isInclude,
        bool isSubroutine)
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

    private static ILuaLibConfigStore ResolveConfigStore() =>
        ((App)Application.Current).Services.GetRequiredService<ILuaLibConfigStore>();

    private static ILuaImportSettingsStore ResolveSettingsStore() =>
        ((App)Application.Current).Services.GetRequiredService<ILuaImportSettingsStore>();
}
