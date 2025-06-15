using System.IO;
using MapCoreLib.Core.Util;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.Utils;

namespace Ra3MapUtils.Services.Impl;

public class MapDataOperateService: IMapDataOperateService
{
    
    public string ExportMapPlayerDataAsJsonStr(string sourceMapName)
    {
        var ra3MapFacade = Ra3MapFacade.Ra3MapFacade.Open(PathUtil.RA3MapFolder, sourceMapName);
        return ra3MapFacade.ExportPlayersToJsonStr();
    }

    public string ExportMapTeamDataAsJsonStr(string sourceMapName)
    {
        var ra3MapFacade = Ra3MapFacade.Ra3MapFacade.Open(PathUtil.RA3MapFolder, sourceMapName);
        return ra3MapFacade.ExportTeamsToJsonStr();
    }

    public void ImportMapPlayerDataFromJsonStr(string jsonStr, string targetMapName)
    {
        var ra3MapFacade = Ra3MapFacade.Ra3MapFacade.Open(PathUtil.RA3MapFolder, targetMapName);
        ra3MapFacade.ImportPlayersFromJsonStr(jsonStr);
        ra3MapFacade.Save();
    }

    public void ImportMapTeamDataFromJsonStr(string jsonStr, string targetMapName)
    {
        var ra3MapFacade = Ra3MapFacade.Ra3MapFacade.Open(PathUtil.RA3MapFolder, targetMapName);
        ra3MapFacade.ImportTeamsFromJsonStr(jsonStr);
        ra3MapFacade.Save();
    }
}