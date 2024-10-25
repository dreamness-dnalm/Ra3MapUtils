using System.Collections.Generic;
using System.Xml.Linq;

namespace UtilLib.mapXmlOperator
{
    public static class MapXmlHelper
    {
        public static XElement MakeScript(string name, List<string> luaContents)
        {
            var script = new XElement("Script", new XAttribute("Name", name));

            var ifEle = new XElement("If");
            script.Add(ifEle);
            var orCondition = new XElement("OrCondition");
            ifEle.Add(orCondition);
            orCondition.Add(new XElement("CONDITION_TRUE"));

            var then = new XElement("Then");
            script.Add(then);
            foreach(var content in luaContents)
            {
                var debugMessageBox = new XElement("DEBUG_MESSAGE_BOX");
                then.Add(debugMessageBox);
                debugMessageBox.Add(new XElement("Text_0", new XAttribute("value", "#!ra3luabridge\n\r" + content)));
            }
            
            script.Add(new XElement("Else"));

            return script;
        }
    }
}