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
        
        private MapXmlOperator(string xmlPath)
        {
            _document = XElement.Load(xmlPath);
            _mapScript = _document.Element(MapXmlHelper.GetXName("MapScript"));
            if(_mapScript == null)
            {
                _document.Add(new XElement(MapXmlHelper.GetXName("MapScript")));
                _mapScript = _document.Element(MapXmlHelper.GetXName("MapScript"));
            }
            _firstScriptList = _mapScript.Element(MapXmlHelper.GetXName("ScriptList"));
            if(_firstScriptList == null)
            {
                _mapScript.Add(new XElement(MapXmlHelper.GetXName("ScriptList")));
                _firstScriptList = _mapScript.Element(MapXmlHelper.GetXName("ScriptList"));
            }
        }
        
        public void RemoveScriptGroup(string scriptGroupName)
        {
            var xElements = _firstScriptList.Elements(MapXmlHelper.GetXName("ScriptGroup"))
                .Where(x => x.Attribute("Name").Value == scriptGroupName);

            foreach (var xElement in xElements)
            {
                xElement.Remove();
            }
        }
        
        public void AddScriptGroup(XElement scriptGroupXElement, string behindScriptGroupName)
        {
            var scriptXElements = _firstScriptList.Elements(MapXmlHelper.GetXName("Script")).ToList();
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
                var behindScriptGroup = _firstScriptList.Elements(MapXmlHelper.GetXName("ScriptGroup"))
                    .Where(x => x.Attribute("Name").Value == behindScriptGroupName).FirstOrDefault();
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