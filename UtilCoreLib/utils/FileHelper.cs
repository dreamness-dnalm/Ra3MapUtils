namespace UtilLib.utils
{
    public class FileHelper
    {
        public static string GetFileContent(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}