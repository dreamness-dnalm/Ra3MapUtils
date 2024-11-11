using Ra3MapUtils.Views;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public static class GlobalVarsModel
{
    public static bool LuaManagerWindowOpened { get; private set; } = false;

    public static void SetLuaManagerWindowOpenedMapName(string mapName)
    {
        LuaImporterBusiness.ActiveMapName = mapName;
    }
}