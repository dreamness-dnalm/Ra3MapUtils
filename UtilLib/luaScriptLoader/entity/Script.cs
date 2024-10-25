using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity
{
    [Serializable]
    public class Script: ScriptUnit
    {
        [XmlElement(typeof(LuaScript), ElementName = "LuaScript")]
        public List<LuaScript> LuaScripts { get; set; } 
    }
}
