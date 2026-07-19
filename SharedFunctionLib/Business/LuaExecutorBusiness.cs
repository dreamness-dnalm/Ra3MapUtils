using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public static class LuaExecutorBusiness
{
    private const string EnableLuaLibraryKey = "LuaExecutor_EnableLuaLibrary";
    private const string UserCodeLibraryPathKey = "LuaExecutor_UserCodeLibraryPath";

    public static bool EnableLuaLibrary
    {
        get
        {
            var value = SettingsDAO.GetSetting(EnableLuaLibraryKey);
            return value == null || bool.TryParse(value, out var enabled) && enabled;
        }
        set => SettingsDAO.SetSetting(EnableLuaLibraryKey, value.ToString());
    }

    public static string UserCodeLibraryPath
    {
        get => SettingsDAO.GetSetting(UserCodeLibraryPathKey) ?? "";
        set => SettingsDAO.SetSetting(UserCodeLibraryPathKey, value ?? "");
    }
}
