using System.Xml.Linq;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Services.Impl;

public class LuaImportService: ILuaImportService
{
    public void SaveMapLuaLibConfig(LuaLibConfigModel luaLibConfigModel)
    {
        LuaImporterBusiness.Save(luaLibConfigModel.MapName, luaLibConfigModel.ShowingName, luaLibConfigModel.LibPath, luaLibConfigModel.OrderNum);
    }

    public void RenameMapLuaLibConfig(string mapName, string oldShowingName, string newShowingName)
    {
        LuaImporterBusiness.Rename(mapName, oldShowingName, newShowingName);
    }

    public void DeleteMapLuaLibConfig(string mapName, string showingName)
    {
        LuaImporterBusiness.Delete(mapName, showingName);
    }

    public List<LuaLibConfigModel> LoadMapLuaLibConfig(string mapName)
    {
        return LuaImporterBusiness.Load(mapName).Select(i => LuaLibConfigModel.FromSimple(i)).ToList();
    }
    
}