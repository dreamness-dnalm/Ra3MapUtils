using System.Diagnostics;
using UtilLib.utils;

namespace Ra3MapUtils.Utils;

public class ProgramUtil
{
    public static async Task<bool> Run(string path)
    {
        try
        {
            Process.Start(path);
            return true;
        }catch(Exception e)
        {
            Logger.WriteLog("open program failed: " + e.Message);
            return false;
        }
        
    }
}