using System;
using System.Xml.Linq;

namespace UtilLib.mapXmlOperator
{
    public class MapXmlOperator
    {
        private XDocument _document;
        private XElement _root;
        private XElement _mapScript;
        
        private MapXmlOperator(string xmlPath)
        {
            _document = XDocument.Load(xmlPath);
            _root = _document.Element("Ra3Map");
            if(_root == null)
            {
                throw new Exception("Invalid map xml file: " + xmlPath);
            }
            _mapScript = _root.Element("MapScript");
            if(_mapScript == null)
            {
                _root.Add(new XElement("MapScript"));
                _mapScript = _root.Element("MapScript");
            }
        }
        
        
        
        
        public static MapXmlOperator Load(string xmlPath)
        {
            return new MapXmlOperator(xmlPath);
        }
        
        public void Save()
        {
            
        }
    }
}