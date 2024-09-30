using System;
using System.Collections.Generic;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;

namespace UtilLib.luaScriptLoader
{
    public class MapScriptGroupListener: MapListener
    {
        public List<ScriptList> OutScriptLists = new List<ScriptList>();
        
        public override List<string> interestedIn()
        {
            return new List<string>()
            {
                "PlayerScriptsList"
            };
        }

        // public override void visitScriptGroup(MapDataContext context, ScriptGroup scriptGroup)
        // {
        //     if (OutScriptGroup == null)
        //     {
        //         OutScriptGroup = scriptGroup;
        //     }
        //     Console.WriteLine("visitScriptGroup, " + scriptGroup.Name);
        // }

        // public override void visitScript(MapDataContext context, Script script)
        // {
        //     Console.WriteLine("visitScript, " + script.Name);
        //     
        // }

        // public override void visitScriptAction(MapDataContext context, ScriptAction scriptAction)
        // {
        //     Console.WriteLine("visitScriptAction," );
        // }

        // public override void visitScriptCondition(MapDataContext context, ScriptCondition scriptCondition)
        // {
        //     Console.WriteLine("visitScriptCondition, " + scriptCondition.scriptContent.contentName);
        // }

        public override void visitScriptList(MapDataContext context, ScriptList scriptList, int index)
        {
            
            // scriptList.scripts[0]  一个文件夹
            
            // Console.WriteLine("visitScriptList," + scriptList + ", index: " + index);
            if (!OutScriptLists.Contains(scriptList))
            {
                OutScriptLists.Add(scriptList);
            }
        }

        // public override void visitScriptActionFalse(MapDataContext context, ScriptAction scriptActionFalse)
        // {
        //     Console.WriteLine("visitScriptActionFalse," + scriptActionFalse.getAssetName());
        // }
    }
}

