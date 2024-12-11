using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        if (IsTableExists())
        {
            Upgrade();
        }
        conn.Execute(@"
            create table if not exists lua_import_config
            (
                id         integer primary key autoincrement,
                MapName    text not null,
                ShowingName text not null,
                LibPath    text not null,
                OrderNum       integer not null,
                IsEnabled  INTEGER  not null default 1,
                UNIQUE(MapName, ShowingName)
            )
        ");
    }

    public static void Save(SimpleLuaLibConfigModel model)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
                     insert or replace into lua_import_config (MapName, ShowingName, LibPath, OrderNum, IsEnabled)
                        values (@MapName, @ShowingName, @LibPath, @OrderNum, @IsEnabled)
                        ", new DataParameter[]
        {
            new DataParameter("MapName", model.MapName), 
            new DataParameter("ShowingName", model.ShowingName), 
            new DataParameter("LibPath", model.LibPath),
            new DataParameter("OrderNum", model.OrderNum),
            new DataParameter("IsEnabled", model.IsEnabled)
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

    private static void Upgrade()
    {
        using var conn = SqliteConnection.GetConnection();
        
        // 获取表的字段信息
        DataTable schema = conn.Connection.GetSchema("Columns", new[] { null, null, "lua_import_config", null });
        List<string> columns = new List<string>();
        foreach (DataRow row in schema.Rows)
        {
            columns.Add(row["COLUMN_NAME"].ToString());
        }
        // Console.WriteLine(columns.Contains("IsEnabled"));
        // Console.WriteLine(columns.Contains("MapName"));
        if (!columns.Contains("IsEnabled"))
        {
            // 添加字段
            conn.Execute(@"
                alter table lua_import_config add column IsEnabled INTEGER not null default 1
            ");
        }
    }

    private static bool IsTableExists()
    {
        using var conn = SqliteConnection.GetConnection();
        DataTable tables = conn.Connection.GetSchema("Tables");

        // 检查表是否存在
        var tableName = "lua_import_config";
        foreach (DataRow row in tables.Rows)
        {
            if(row["TABLE_NAME"].ToString() == tableName)
            {
                return true;
            }
        }
        return false;
    }
}