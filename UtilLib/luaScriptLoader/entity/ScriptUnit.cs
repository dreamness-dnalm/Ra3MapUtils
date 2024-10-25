using System;
using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity
{
    [Serializable]
    [XmlInclude(typeof(Script))]
    [XmlInclude(typeof(Folder))]
    [XmlInclude(typeof(LuaScript))]
    public class ScriptUnit
    {
        [XmlAttribute("Name")] public string Name { get; set; }

        [XmlAttribute("Enabled")] public bool Enabled { get; set; } = true;
    }
}
