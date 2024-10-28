using System;
using System.Linq;
using System.Xml.Linq;
using MapCoreLib.Core.Asset;

namespace UtilLib.mapXmlOperator
{
    public class MapXmlOperator
    {
        private XElement _document;
        private XElement _mapScript;
        private XElement _firstScriptList;
        
        public static readonly string NAMESPACE = "uri:wu.com:ra3map";
        
        private MapXmlOperator(string xmlPath)
        {
            _document = XElement.Load(xmlPath);
            _mapScript = _document.Element(XName.Get("MapScript", NAMESPACE));
            if(_mapScript == null)
            {
                _document.Add(new XElement("MapScript"));
                _mapScript = _document.Element(XName.Get("MapScript", NAMESPACE));
            }
            _firstScriptList = _mapScript.Element(XName.Get("ScriptList", NAMESPACE));
            if(_firstScriptList == null)
            {
                _mapScript.Add(new XElement("ScriptList"));
                _firstScriptList = _mapScript.Element(XName.Get("ScriptList", NAMESPACE));
            }
        }
        
        public void RemoveScriptGroup(string scriptGroupName)
        {
            var xElements = _firstScriptList.Elements(XName.Get("ScriptGroup", NAMESPACE))
                .Where(x => x.Attribute("Name").Value == scriptGroupName);

            foreach (var xElement in xElements)
            {
                xElement.Remove();
            }
        }
        
        public void AddScriptGroup(XElement scriptGroupXElement, string behindScriptGroupName)
        {
            var scriptXElements = _firstScriptList.Elements(XName.Get("Script", NAMESPACE)).ToList();
            XElement lastScriptXElement = null;
            if (scriptXElements.Count > 0)
            {
                lastScriptXElement = scriptXElements.Last();
            }
            if (behindScriptGroupName == null)
            {
                if (lastScriptXElement != null)
                {
                    lastScriptXElement.AddAfterSelf(scriptGroupXElement);
                }
                else
                {
                    _firstScriptList.Add(scriptGroupXElement);
                }
            }
            else
            {
                var behindScriptGroup = _firstScriptList.Elements(XName.Get("ScriptGroup", NAMESPACE))
                    .Where(x => x.Attribute("Name").ToString() == behindScriptGroupName).FirstOrDefault();
                if (behindScriptGroup == null)
                {
                    if (lastScriptXElement != null)
                    {
                        lastScriptXElement.AddAfterSelf(scriptGroupXElement);
                    }
                    else
                    {
                        _firstScriptList.Add(scriptGroupXElement);
                    }
                }
                else
                {
                    behindScriptGroup.AddAfterSelf(scriptGroupXElement);
                }
                
            }
        }
        
        
        
        public static MapXmlOperator Load(string xmlPath)
        {
            return new MapXmlOperator(xmlPath);
        }
        
        public void Save(string xmlPath)
        {
            _document.Save(xmlPath);
        }
    }
}