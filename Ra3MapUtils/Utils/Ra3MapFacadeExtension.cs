using System.IO;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.Util;
using SharedFunctionLib.Utils;

namespace Ra3MapUtils.Utils;

public static class Ra3MapFacadeExtension
{
    public static string Backup(this Ra3MapFacade ra3map)
    {
        var backupDir = Path.Combine(Ra3MapUtilsPathUtil.BackupFolderPath, "map");
        Directory.CreateDirectory(backupDir);

        var mapName = Path.GetFileNameWithoutExtension(ra3map.ra3Map.MapFilePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFile = $"backup_{mapName}_{timestamp}.map";
        var backupPath = Path.Combine(backupDir, backupFile);

        ra3map.SaveAs(backupPath);
        return backupPath;
    }
}