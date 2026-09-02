using Core.Paths;
using Microsoft.Data.Sqlite;

namespace Core.LuaImport;

public sealed class SqliteLuaImportSettingsStore : ILuaImportSettingsStore
{
    public const string RedundancyFactorKey = "LuaImporter_LuaRedundancyFactor";
    public const string ActiveMapNameKey = "LuaImporter_ActiveMapName";
    public const string ActiveMapFilePathKey = "LuaImporter_ActiveMapFilePath";
    public const int DefaultRedundancyFactor = 100;

    private readonly string _dbPath;
    private readonly object _gate = new();

    public SqliteLuaImportSettingsStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppDataPaths.V2SqliteDbPath;
    }

    public int GetRedundancyFactor()
    {
        lock (_gate)
        {
            EnsureSchema();
            var raw = GetSetting(RedundancyFactorKey);
            if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out var factor) || factor < 0)
            {
                SetSetting(RedundancyFactorKey, DefaultRedundancyFactor.ToString());
                return DefaultRedundancyFactor;
            }

            return factor;
        }
    }

    public void SetRedundancyFactor(int factor)
    {
        if (factor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Redundancy factor must be non-negative.");
        }

        lock (_gate)
        {
            EnsureSchema();
            SetSetting(RedundancyFactorKey, factor.ToString());
        }
    }

    public string? GetActiveMapName()
    {
        lock (_gate)
        {
            EnsureSchema();
            return NullIfEmpty(GetSetting(ActiveMapNameKey));
        }
    }

    public string? GetActiveMapFilePath()
    {
        lock (_gate)
        {
            EnsureSchema();
            return NullIfEmpty(GetSetting(ActiveMapFilePathKey));
        }
    }

    public void SetActiveMap(string? mapName, string? mapFilePath)
    {
        lock (_gate)
        {
            EnsureSchema();
            if (mapName is null)
            {
                DeleteSetting(ActiveMapNameKey);
            }
            else
            {
                SetSetting(ActiveMapNameKey, mapName);
            }

            if (mapFilePath is null)
            {
                DeleteSetting(ActiveMapFilePathKey);
            }
            else
            {
                SetSetting(ActiveMapFilePathKey, mapFilePath);
            }
        }
    }

    public void ClearActiveMap() => SetActiveMap(null, null);

    private string? GetSetting(string key)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT SettingValue FROM ui_settings WHERE SettingKey = $key LIMIT 1;";
        cmd.Parameters.AddWithValue("$key", key);
        return cmd.ExecuteScalar() as string;
    }

    private void SetSetting(string key, string value)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO ui_settings (SettingKey, SettingValue)
            VALUES ($key, $value)
            ON CONFLICT(SettingKey) DO UPDATE SET SettingValue = excluded.SettingValue;
            """;
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$value", value);
        cmd.ExecuteNonQuery();
    }

    private void DeleteSetting(string key)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ui_settings WHERE SettingKey = $key;";
        cmd.Parameters.AddWithValue("$key", key);
        cmd.ExecuteNonQuery();
    }

    private void EnsureSchema()
    {
        AppDataPaths.EnsureUserDataLayout();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS ui_settings (
                SettingKey TEXT PRIMARY KEY NOT NULL,
                SettingValue TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        return conn;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
