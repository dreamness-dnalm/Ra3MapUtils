using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity
{
    [Serializable]
    public class Folder: ScriptUnit
    {
        [XmlElement(typeof(Folder), ElementName = "Folder")]
        [XmlElement(typeof(Script), ElementName = "Script")]
        public List<ScriptUnit> Children { get; set; }
    }
}
