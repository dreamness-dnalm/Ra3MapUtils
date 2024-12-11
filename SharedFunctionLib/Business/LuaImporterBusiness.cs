using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using SharedFunctionLib.DAO;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;
using SharedFunctionLib.Models;
using SharedFunctionLib.Utils;
using UtilCoreLib.mapScriptHelper;
using UtilLib.utils;

namespace SharedFunctionLib.Business;

public static class LuaImporterBusiness
{
    public static string? ActiveMapName
    {
        get => SettingsDAO.GetSetting("LuaImporter_ActiveMapName");
        set
        {
            if (value == null)
            {
                SettingsDAO.DeleteSetting("LuaImporter_ActiveMapName");
            }
            else
            {
                SettingsDAO.SetSetting("LuaImporter_ActiveMapName", value);
            }
            
        }
    }
    
    public static int LuaRedundancyFactor
    {
        get
        {
            var redundancyFactor = SettingsDAO.GetSetting("LuaImporter_LuaRedundancyFactor");
            if (redundancyFactor == null)
            {
                LuaRedundancyFactor = 100;
                return 100;
            }
            return int.Parse(redundancyFactor);
        }
        set => SettingsDAO.SetSetting("LuaImporter_LuaRedundancyFactor", value.ToString());
    }

    public static List<SimpleLuaLibConfigModel> LoadLuaLibConfigModels()
    {
        var mapName = ActiveMapName;
        if (mapName == null)
        {
            throw new Exception("No Map Is Active In LuaImporter. Are you sure you have open then LuaImporter Window?");
        }
        Logger.WriteLog("ActiveMapName: " + mapName);
        var luaLibConfigModels = Load(mapName)
            .Where(i => i.LibPath != null && i.LibPath != "")
            .OrderBy(i => i.OrderNum).ToList();
        Logger.WriteLog("LoadLuaLibConfigModels Finished.");
        return luaLibConfigModels;
    }
    
    public static void Save(string mapName, string showingName, string libPath, int orderNum, bool isEnabled)
    {
        var model = new SimpleLuaLibConfigModel
        {
            MapName = mapName,
            ShowingName = showingName,
            LibPath = libPath,
            OrderNum = orderNum,
            IsEnabled = isEnabled ? 1 : 0
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
}