namespace Ra3MapUtils.Services.Interface;

public interface IMapDataOperateService
{
    public string ExportMapPlayerDataAsJsonStr(string sourceMapName);
    
    public string ExportMapTeamDataAsJsonStr(string sourceMapName);
    
    public string ExportMapScriptsDataAsJsonStr(string sourceMapName);
    
    public void ImportMapPlayerDataFromJsonStr(string jsonStr, string targetMapName);
    
    public void ImportMapTeamDataFromJsonStr(string jsonStr, string targetMapName);
    
    public void ImportMapScriptsDataFromJsonStr(string jsonStr, string targetMapName);
    

}