using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity;
[Serializable]
public class LuaScript
{
    [XmlAttribute("Path")]
    public string Path { get; set; }
}