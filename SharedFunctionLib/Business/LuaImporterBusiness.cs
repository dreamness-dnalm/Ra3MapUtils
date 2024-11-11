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

    public static List<SimpleLuaLibConfigModel> LoadLuaLibConfigModels()
    {
        Logger.WriteLog("start ImportActiveMapLua");
        // var mapName = ActiveMapName;
        // if (mapName == null)
        // {
        //     throw new Exception("No Map Is Active In LuaImporter. Are you sure you have open then LuaImporter Window?");
        // }
        var mapName = "NewMap19";
        
        var luaLibConfigModels = Load(mapName)
            .Where(i => i.LibPath != null && i.LibPath != "")
            .OrderBy(i => i.OrderNum).ToList();
        Logger.WriteLog("got lua lib config models");
        return luaLibConfigModels;
    }

    public static void ImportActiveMapLua(MapDataContext context)
    {
        Logger.WriteLog("start ImportActiveMapLua");
        // var mapName = ActiveMapName;
        // if (mapName == null)
        // {
        //     throw new Exception("No Map Is Active In LuaImporter. Are you sure you have open then LuaImporter Window?");
        // }
        var mapName = "NewMap19";
        
        var scriptList = context.getAsset<PlayerScriptsList>("PlayerScriptsList").scriptLists[0];
        Logger.WriteLog("got script list");
        var luaLibConfigModels = Load(mapName)
            .Where(i => i.LibPath != null && i.LibPath != "")
            .OrderBy(i => i.OrderNum).ToList();
        Logger.WriteLog("got lua lib config models");
        string lastScriptGroupName = null;
        for (int i = 0; i < luaLibConfigModels.Count; i++)
        {
            var currModel = luaLibConfigModels[0];

            var fileModel = SimpleLibFileModel.Load(currModel.LibPath);
            Logger.WriteLog("fileModel loaded");
            var scriptGroup = fileModel.Translate(context).Item1;
            Logger.WriteLog("translate finished");
            if (scriptGroup != null)
            {
                MapScriptOperator.AddScriptGroup(context, scriptList, (ScriptGroup)scriptGroup, lastScriptGroupName);
                lastScriptGroupName = Path.GetFileName(currModel.LibPath);
            }
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
        var mapName = "NewMap19";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.modifyMap();
        ra3Map.parse();
        
        var context = ra3Map.getContext();
        
        ImportActiveMapLua(context);
        
        ra3Map.doSaveMap(ra3Map.mapPath);
    }
}