using MapCoreLib.Core;
using MapCoreLib.Core.Asset;

namespace UtilCoreLib.mapScriptHelper;

public static class MapScriptOperator
{
    public static void AddScriptGroup(MapDataContext context, ScriptList scriptList, ScriptGroup scriptGroup, string behindScriptGroupName)
    {
        RemoveScriptGroup(scriptList, scriptGroup.Name);
        
        // scriptList.scriptGroups.IndexOf(i => i.Name == behindScriptGroupName)
        //     .IfExist(i => scriptList.scriptGroups.Insert(i, scriptGroup))
        //     .Else(() => scriptList.scriptGroups.Add(scriptGroup));
        
        scriptList.scriptGroups.Add(scriptGroup);
        
        scriptGroup.registerSelf(context);
    }

    public static void RemoveScriptGroup(ScriptList scriptList, string scriptGroupName)
    {
        scriptList.scriptGroups.Where(i => i.Name == scriptGroupName).ToList()
            .ForEach(i => scriptList.scriptGroups.Remove(i));
    }
}