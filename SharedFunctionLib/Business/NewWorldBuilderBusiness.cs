using System;
using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public static class NewWorldBuilderBusiness
{
    public static string NewWorldBuilderPath
    {
        get
        {
            var newWorldBuilderPath = SettingsDAO.GetSetting("NewWorldBuilder_NewWorldBuilderPath");
            if (newWorldBuilderPath == null)
            {
                NewWorldBuilderPath = "";
                return "";
            }
            return newWorldBuilderPath;
        }
        set => SettingsDAO.SetSetting("NewWorldBuilder_NewWorldBuilderPath", value);
    }
    
    public static bool IsPathValidForNewWorldBuilderPath(string path)
    {
        try
        {
            return path.EndsWith("WbLauncher.exe") && System.IO.File.Exists(path);
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    public static bool IsNewWorldBuilderPathValid
    {
        get
        {
            return IsPathValidForNewWorldBuilderPath(NewWorldBuilderPath);
        }
    }
}