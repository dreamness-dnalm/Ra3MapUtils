using System.IO;
using LinqToDB;
using LinqToDB.Data;
using SharedFunctionLib.Models;

namespace SharedFunctionLib.Utils;

public class SqliteConnection: DataConnection
{
    private SqliteConnection() : base("SQLite", $"Data Source={Ra3MapUtilsPathUtil.SqliteDBPath};Version=3;")
    {
    }
    
    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection();
    }
    
    public ITable<SimpleLuaLibConfigModel> LuaLibConfigModels => this.GetTable<SimpleLuaLibConfigModel>();
    public ITable<SimpleSettingModel> Settings => this.GetTable<SimpleSettingModel>();
}