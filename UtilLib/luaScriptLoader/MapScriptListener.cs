using MapCoreLib.Core;
using MapCoreLib.Core.Asset;

namespace UtilLib.luaScriptLoader;

public class MapScriptListener: MapListener
{
    public override List<string> interestedIn()
    {
        return new List<string>()
        {
            "PlayerScriptsList"
        };
    }

    public override void visitScriptGroup(MapDataContext context, ScriptGroup scriptGroup)
    {
        base.visitScriptGroup(context, scriptGroup);
    }

    public override void visitScript(MapDataContext context, Script script)
    {
        base.visitScript(context, script);
    }

    public override void visitScriptAction(MapDataContext context, ScriptAction scriptAction)
    {
        base.visitScriptAction(context, scriptAction);
    }

    public override void visitScriptCondition(MapDataContext context, ScriptCondition scriptCondition)
    {
        base.visitScriptCondition(context, scriptCondition);
    }

    public override void visitScriptList(MapDataContext context, ScriptList scriptList, int index)
    {
        base.visitScriptList(context, scriptList, index);
    }

    public override void visitScriptActionFalse(MapDataContext context, ScriptAction scriptActionFalse)
    {
        base.visitScriptActionFalse(context, scriptActionFalse);
    }
}