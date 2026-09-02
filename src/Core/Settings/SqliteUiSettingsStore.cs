using Core.Paths;
using Microsoft.Data.Sqlite;

namespace Core.Settings;

public sealed class SqliteUiSettingsStore : IUiSettingsStore
{
    public const string LanguageKey = "UI_Language";

    private readonly string _dbPath;
    private readonly object _gate = new();

    public SqliteUiSettingsStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppDataPaths.V2SqliteDbPath;
    }

    public string? GetLanguageOrNull()
    {
        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT SettingValue FROM ui_settings WHERE SettingKey = $key LIMIT 1;";
            cmd.Parameters.AddWithValue("$key", LanguageKey);
            var value = cmd.ExecuteScalar() as string;
            if (string.IsNullOrWhiteSpace(value) || !UiLanguageCodes.IsSupported(value))
            {
                return null;
            }

            return UiLanguageCodes.Normalize(value);
        }
    }

    public void SetLanguage(string languageCode)
    {
        if (!UiLanguageCodes.IsSupported(languageCode))
        {
            throw new ArgumentException("Unsupported language code.", nameof(languageCode));
        }

        var normalized = UiLanguageCodes.Normalize(languageCode);
        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO ui_settings (SettingKey, SettingValue)
                VALUES ($key, $value)
                ON CONFLICT(SettingKey) DO UPDATE SET SettingValue = excluded.SettingValue;
                """;
            cmd.Parameters.AddWithValue("$key", LanguageKey);
            cmd.Parameters.AddWithValue("$value", normalized);
            cmd.ExecuteNonQuery();
        }
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
}
