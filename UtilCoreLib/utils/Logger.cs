using System.IO;

namespace UtilLib.utils;

public class Logger
{
    private static readonly string logPath = "log.txt";
    
    public static void WriteLog(string log)
    {
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine(log);
        }
    }
}