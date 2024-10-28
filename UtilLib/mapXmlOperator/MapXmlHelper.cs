using System.Collections.Generic;
using System.Xml.Linq;

namespace UtilLib.mapXmlOperator
{
    public static class MapXmlHelper
    {
        public static XElement MakeScript(string name, List<string> luaContents, bool isEnabled, bool isInclude, bool runOnce)
        {
            if (! isInclude)
            {
                return null;
            }
            
            var script = new XElement("Script", 
                new XAttribute("Name", name), 
                new XAttribute("isActive", isEnabled?"true":"false"), 
                new XAttribute("DeactivateUponSuccess", runOnce?"true":"false"));

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
        
        public static XElement MakeScriptGroup(string name, List<XElement> subScripts, List<XElement> subScriptGroups, bool isEnabled, bool isInclude)
        {
            if (!isInclude)
            {
                return null;
            }
            
            var scriptGroup = new XElement(XName.Get("ScriptGroup", "uri:wu.com:ra3map"), new XAttribute("Name", name), new XAttribute("IsActive", isEnabled?"true":"false"));
            foreach (var subScript in subScripts)
            {
                scriptGroup.Add(subScript);
            }

            foreach (var subScriptGroup in subScriptGroups)
            {
                scriptGroup.Add(subScriptGroup);
            }

            return scriptGroup;
        }
    }
}