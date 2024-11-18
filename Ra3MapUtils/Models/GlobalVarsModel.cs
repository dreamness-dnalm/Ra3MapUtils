using System.IO;
using Ra3MapUtils.Utils;
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

    public static string ProgramName = "RA3地编伴侣";

    private static string _versionStr;

    public static string VersionStr
    {
        get
        {
            if (_versionStr == null)
            {
                _versionStr = "v" + EmbeddedResourcesUtil.GetEmbeddedResourceContent("Ra3MapUtils.VERSION");
            }

            return _versionStr;
        }
    }

    public static NuGet.Versioning.SemanticVersion Version => NuGet.Versioning.SemanticVersion.Parse(VersionStr);
}