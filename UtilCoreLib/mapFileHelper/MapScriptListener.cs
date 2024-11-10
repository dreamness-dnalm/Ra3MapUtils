using System;
using System.Collections.Generic;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;

namespace UtilLib.mapFileHelper
{
    public class MapScriptListener: MapListener
    {

        public ScriptList Value;

        public override List<string> interestedIn()
        {
            return new List<string>()
            {
                "PlayerScriptsList"
            };
        }

        public override void visitScriptList(MapDataContext context, ScriptList scriptList, int index)
        {
            
            if(index == 0)
            {
                Value = scriptList;
            }
        }
    }
}