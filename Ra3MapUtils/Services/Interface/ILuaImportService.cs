using System.Xml.Linq;
using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

public interface ILuaImportService
{
    public void SaveMapLuaLibConfig(LuaLibConfigModel luaLibConfigModel);
    
    public void RenameMapLuaLibConfig(string mapName, string oldShowingName, string newShowingName);
    
    public void DeleteMapLuaLibConfig(string mapName, string showingName);
    
    public List<LuaLibConfigModel> LoadMapLuaLibConfig(string mapName);
    
    // public void UpsertScriptGroup(string mapName, XElement scriptGroupXElement, string behindScriptGroupName);
}