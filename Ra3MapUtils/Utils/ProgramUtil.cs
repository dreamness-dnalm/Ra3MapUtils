using System.Diagnostics;
using System.IO;
using UtilLib.utils;

namespace Ra3MapUtils.Utils;

public class ProgramUtil
{
    public static async Task<bool> Run(string path, string? workingDirectory = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo(path)
            {
                WorkingDirectory = !string.IsNullOrWhiteSpace(workingDirectory)
                    ? workingDirectory
                    : (Path.GetDirectoryName(path) ?? Environment.CurrentDirectory)
            };
            Process.Start(startInfo);
            return true;
        }catch(Exception e)
        {
            Logger.WriteLog("open program failed: " + e.Message);
            return false;
        }
        
    }
}
