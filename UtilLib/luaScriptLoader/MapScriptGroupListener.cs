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
        

        public override void visitScriptList(MapDataContext context, ScriptList scriptList, int index)
        {
                OutScriptLists.Add(scriptList);
        }
    }
}

