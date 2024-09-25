using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity;
[Serializable]
public class Script: ScriptUnit
{
    [XmlElement("LuaScript")]
    List<LuaScript> LuaScripts { get; set; } 
}