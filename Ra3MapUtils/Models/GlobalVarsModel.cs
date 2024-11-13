using System.IO;
using Ra3MapUtils.Views;
using SemVersion;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public static class GlobalVarsModel
{
    public static bool LuaManagerWindowOpened { get; private set; } = false;

    public static void SetLuaManagerWindowOpenedMapName(string mapName)
    {
        LuaImporterBusiness.ActiveMapName = mapName;
    }

    public static string VersionStr = "0.1.0.0-alpha";

    public static SemanticVersion Version => SemanticVersion.Parse(VersionStr);
}