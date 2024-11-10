using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using SharedFunctionLib.DAO;
using MapCoreLib.Core;
using SharedFunctionLib.Models;

namespace SharedFunctionLib.Business;

public static class LuaImporterBusiness
{
    public static string? ActiveMapName
    {
        get => SettingsDAO.GetSetting("LuaImporter_ActiveMapName");
        set => SettingsDAO.SetSetting("LuaImporter_ActiveMapName", value);
    }

    public static void ImportActiveMapLua(MapDataContext context)
    {
        var mapName = ActiveMapName;
        if (mapName == null)
        {
            throw new Exception("No Map Is Active In LuaImporter. Are you sure you have open then LuaImporter Window?");
        }
    }
    
    public static void Save(string mapName, string showingName, string libPath, int orderNum)
    {
        var model = new SimpleLuaLibConfigModel
        {
            MapName = mapName,
            ShowingName = showingName,
            LibPath = libPath,
            OrderNum = orderNum
        };
        LuaLibConfigDAO.Save(model);
    }

    public static void Rename(string mapName, string oldShowingName, string newShowingName)
    {
        LuaLibConfigDAO.Rename(mapName, oldShowingName, newShowingName);
    }

    public static void Delete(string mapName, string showingName)
    {
        LuaLibConfigDAO.Delete(mapName, showingName);
    }
    
    public static List<SimpleLuaLibConfigModel> Load(string mapName)
    {
        return LuaLibConfigDAO.Load(mapName);
    }

    public static void Main()
    {
        var a = ActiveMapName;
        Console.WriteLine(a);
    }
}