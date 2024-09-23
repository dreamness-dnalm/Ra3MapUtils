using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity;
[Serializable]
[XmlInclude(typeof(Script))]
[XmlInclude(typeof(Folder))]
public class ScriptUnit
{
    [XmlAttribute("Name")]
    public string Name { get; set; }
    [XmlAttribute("Enabled")]
    public bool Enabled { get; set; }
}