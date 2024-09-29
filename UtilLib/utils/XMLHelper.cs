using System.IO;
using System.Xml.Serialization;

namespace UtilLib.utils
{
    public static class XMLHelper
    {
        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}

