using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UtilLib.luaScriptLoader.entity
{
    [Serializable]
    public class LuaScriptLoader: Folder
    {
        [XmlAttribute("MapPath")]
        public string MapPath { set; get; }
    }
}

