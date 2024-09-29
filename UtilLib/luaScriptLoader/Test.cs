using System;
using System.IO;
using System.Xml.Serialization;
using UtilLib.luaScriptLoader.entity;

namespace UtilLib.luaScriptLoader
{
    public class Test
    {
        public static void Main()
        {
            var path = "H:\\workspace\\dreamness_ra3_tools\\test.xml";
        
        
            XmlSerializer serializer = new XmlSerializer(typeof(LuaScriptLoader));

            // 打开文件并反序列化为对象
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                LuaScriptLoader person = (LuaScriptLoader)serializer.Deserialize(fileStream);
                Console.WriteLine(person);
            }
        }
    }
}

