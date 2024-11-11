using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using SharedFunctionLib.Models;
using SharedFunctionLib.Utils;

namespace SharedFunctionLib.DAO;

public static class SettingsDAO
{
    public static string? GetSetting(string key)
    {
        InitDB();
        using var db = SqliteConnection.GetConnection();
        var setting = db.Settings.FirstOrDefault(s => s.SettingKey == key);
        return setting?.SettingValue;
    }
    
    public static void SetSetting(string key, string value)
    {
        InitDB();
        using var db = SqliteConnection.GetConnection();
        var setting = db.Settings.FirstOrDefault(s => s.SettingKey == key);
        if (setting == null)
        {
            db.Insert(new SimpleSettingModel
            {
                SettingKey = key,
                SettingValue = value
            });
        }
        else
        {
            setting.SettingValue = value;
            db.Execute(@"
        update program_settings set SettingValue = @value where SettingKey = @key
", new DataParameter[]
            {
                new DataParameter("key", key),
                new DataParameter("value", value)
            });
        }
    }
    
    public static void DeleteSetting(string key)
    {
        InitDB();
        using var db = SqliteConnection.GetConnection();
        db.Settings
            .Where(s => s.SettingKey == key)
            .Delete();
    }
    
    public static void InitDB()
    {
        using var conn = SqliteConnection.GetConnection();
        conn.Execute(@"
            create table if not exists program_settings
            (
                id         integer primary key autoincrement,
                SettingKey    text not null,
                SettingValue text not null,
                UNIQUE(SettingKey)
            )
        ");
    }
}