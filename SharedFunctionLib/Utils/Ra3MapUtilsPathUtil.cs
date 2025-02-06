using System;
using System.IO;

namespace SharedFunctionLib.Utils;

public static class Ra3MapUtilsPathUtil
{
    public static string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ra3MapUtils");
    
    public static string UserCachePath = Path.Combine(UserDataPath, "Cache");
    
    public static string SqliteDBPath = Path.Combine(UserDataPath, "Ra3MapUtils.db");
}