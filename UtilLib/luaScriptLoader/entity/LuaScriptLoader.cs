using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity;

[Serializable]
public class LuaScriptLoader
{
    [XmlAttribute("MapParentPath")]
    public string MapParentPath { set; get; } = "DEFAULT";
    
    [XmlAttribute("MapName")]
    public string MapName { set; get; }
    
    [XmlElement("Folder")]
    public List<Folder> Folders { set; get; }

    public static LuaScriptLoader Load(string xmlStr)
    {
        return null;
    }
}