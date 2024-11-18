using System.IO;

namespace UtilLib.utils;

public class Logger
{
    private static readonly string logPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ra3MapUtils"), "log.txt");
    
    public static void WriteLog(string log)
    {
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {log}");
        }
    }
}