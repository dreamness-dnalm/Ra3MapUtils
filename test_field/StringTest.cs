using System.Text;

namespace test_field;

public class StringTest
{
    public static class StringHelper
    {
        public static string TranslateEncoding(string sourceString, string sourceEncoding, string targetEncoding)
        {
            Encoding srcEnc = Encoding.GetEncoding(sourceEncoding);
            Encoding destEnc = Encoding.GetEncoding(targetEncoding);
            byte[] srcBytes = srcEnc.GetBytes(sourceString);
            byte[] destBytes = Encoding.Convert(srcEnc, destEnc, srcBytes);
            return destEnc.GetString(destBytes);
        }

        public static string StringTranslate(string source)
        {
            return TranslateEncoding(source, "utf-8", "ascii");
        }
    }
    
    public static void Main()
    {
        string source = "你好，世界！";
        
        string targetEncoding = "gbk";
        // StringHelper.StringTranslate(source);
        string target = StringHelper.TranslateEncoding(source, "utf-8", targetEncoding);
        // foreach (var i in Encoding.GetEncoding(targetEncoding).get.GetBytes(target))
        // {
        //     Console.WriteLine(i);
        // }
    }
}