using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public static class LuaLibBindingBusiness
{
    public static bool IsAutoUpdateEnabled
    {
        get
        {
            var isAutoUpdateEnabled = SettingsDAO.GetSetting("LuaLibBinding_IsAutoUpdateEnabled");
            if(isAutoUpdateEnabled == null)
            {
                IsAutoUpdateEnabled = true;
                return true;
            }
            return bool.Parse(isAutoUpdateEnabled);
        }
        set => SettingsDAO.SetSetting("LuaLibBinding_IsAutoUpdateEnabled", value.ToString());
    }
    
    public static bool IsAutoLoadWhenImport
    {
        get
        {
            var isAutoLoadWhenImport = SettingsDAO.GetSetting("LuaLibBinding_IsAutoLoadWhenImport");
            if(isAutoLoadWhenImport == null)
            {
                IsAutoLoadWhenImport = true;
                return true;
            }
            return bool.Parse(isAutoLoadWhenImport);
        }
        set => SettingsDAO.SetSetting("LuaLibBinding_IsAutoLoadWhenImport", value.ToString());
    }
    
    public static string LuaLibPath
    {
        get
        {
            var luaLibPath = SettingsDAO.GetSetting("LuaLibBinding_LuaLibPath");
            if (luaLibPath == null)
            {
                LuaLibPath = "";
                return "";
            }
            return luaLibPath;
        }
        set => SettingsDAO.SetSetting("LuaLibBinding_LuaLibPath", value);
    }
}