using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using SharedFunctionLib.Models;
using SharedFunctionLib.Utils;

namespace SharedFunctionLib.DAO;

public static class LuaLibConfigDAO
{
    public static void InitDB()
    {
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
            create table if not exists lua_import_config
            (
                id         integer primary key autoincrement,
                MapName    text not null,
                ShowingName text not null,
                LibPath    text not null,
                OrderNum       integer not null,
                UNIQUE(MapName, ShowingName)
            )
        ");
    }

    public static void Save(SimpleLuaLibConfigModel model)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
                     insert or replace into lua_import_config (MapName, ShowingName, LibPath, OrderNum)
                        values (@MapName, @ShowingName, @LibPath, @OrderNum)
                        ", new DataParameter[]
        {
            new DataParameter("MapName", model.MapName), 
            new DataParameter("ShowingName", model.ShowingName), 
            new DataParameter("LibPath", model.LibPath),
            new DataParameter("OrderNum", model.OrderNum)
        });
    }
    
    public static void Delete(string mapName, string showingName)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.LuaLibConfigModels
            .Where(x => x.MapName == mapName && x.ShowingName == showingName)
            .Delete();
    }
    
    public static void Rename(string mapName, string oldShowingName, string newShowingName)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.LuaLibConfigModels
            .Where(x => x.MapName == mapName && x.ShowingName == oldShowingName)
            .Set(x => x.ShowingName, newShowingName)
            .Update();
    }
    
    public static List<SimpleLuaLibConfigModel> Load(string mapName)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        return conn.LuaLibConfigModels
            .Where(x => x.MapName == mapName)
            .OrderBy(x => x.OrderNum)
            .ToList();
    }
    
    
}