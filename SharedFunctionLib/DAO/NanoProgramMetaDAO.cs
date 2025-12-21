using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using SharedFunctionLib.Models;
using SharedFunctionLib.Utils;

namespace SharedFunctionLib.DAO;

public class NanoProgramMetaDAO
{
    public static void InitDB()
    {
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
            create table if not exists nano_program_meta
            (
                ID text,
                IsEnabled INTEGER not null default 1,
                IsWbVisible INTEGER not null default 1,
                OrderNum integer not null default -1,
                UNIQUE(ID)
            )
        ");
    }
    
    public static void AddOrUpdate(SimpleNanoProgramMetaModel model)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
                     insert or replace into nano_program_meta (ID, IsEnabled, IsWbVisible, OrderNum)
                        values (@ID, @IsEnabled, @IsWbVisible, @OrderNum)
                        ", new DataParameter[]
        {
            new DataParameter("ID", model.ID), 
            new DataParameter("IsEnabled", model.IsEnabled), 
            new DataParameter("IsWbVisible", model.IsWbVisible),
            new DataParameter("OrderNum", model.Order)
        });
    }
    
    public static void Delete(string id)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.NanoProgramMetaModels
            .Where(x => x.ID == id)
            .Delete();
    }
    
    public static void DeleteUnused(List<string> usedIds)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        conn.NanoProgramMetaModels
            .Where(s => !usedIds.Contains(s.ID))
            .Delete();
    }
    
    public static List<SimpleNanoProgramMetaModel> GetAll()
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        return conn.NanoProgramMetaModels
            .OrderBy(s => s.Order)
            .ToList();
    }

    public static SimpleNanoProgramMetaModel Get(string id)
    {
        InitDB();
        using var conn = SqliteConnection.GetConnection();
        return conn.NanoProgramMetaModels
            .FirstOrDefault(s => s.ID == id);
    }
}