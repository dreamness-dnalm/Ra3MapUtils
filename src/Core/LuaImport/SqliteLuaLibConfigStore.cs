using Core.Paths;
using Microsoft.Data.Sqlite;

namespace Core.LuaImport;

public sealed class SqliteLuaLibConfigStore : ILuaLibConfigStore
{
    private readonly string _dbPath;
    private readonly object _gate = new();

    public SqliteLuaLibConfigStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppDataPaths.V2SqliteDbPath;
    }

    public IReadOnlyList<LuaLibConfigRecord> Load(string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
        {
            return Array.Empty<LuaLibConfigRecord>();
        }

        lock (_gate)
        {
            EnsureSchema();
            var list = new List<LuaLibConfigRecord>();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT MapName, ShowingName, LibPath, OrderNum, IsEnabled
                FROM lua_import_config
                WHERE MapName = $map
                ORDER BY OrderNum;
                """;
            cmd.Parameters.AddWithValue("$map", mapName);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new LuaLibConfigRecord
                {
                    MapName = reader.GetString(0),
                    ShowingName = reader.GetString(1),
                    LibPath = reader.GetString(2),
                    OrderNum = (int)reader.GetInt64(3),
                    IsEnabled = (int)reader.GetInt64(4),
                });
            }

            return list;
        }
    }

    public void Save(LuaLibConfigRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (string.IsNullOrWhiteSpace(record.MapName))
        {
            throw new ArgumentException("MapName is required.", nameof(record));
        }

        if (string.IsNullOrWhiteSpace(record.ShowingName))
        {
            throw new ArgumentException("ShowingName is required.", nameof(record));
        }

        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO lua_import_config (MapName, ShowingName, LibPath, OrderNum, IsEnabled)
                VALUES ($map, $name, $path, $order, $enabled)
                ON CONFLICT(MapName, ShowingName) DO UPDATE SET
                    LibPath = excluded.LibPath,
                    OrderNum = excluded.OrderNum,
                    IsEnabled = excluded.IsEnabled;
                """;
            cmd.Parameters.AddWithValue("$map", record.MapName);
            cmd.Parameters.AddWithValue("$name", record.ShowingName);
            cmd.Parameters.AddWithValue("$path", record.LibPath ?? "");
            cmd.Parameters.AddWithValue("$order", record.OrderNum);
            cmd.Parameters.AddWithValue("$enabled", record.IsEnabled);
            cmd.ExecuteNonQuery();
        }
    }

    public void Rename(string mapName, string oldShowingName, string newShowingName)
    {
        if (string.IsNullOrWhiteSpace(mapName)
            || string.IsNullOrWhiteSpace(oldShowingName)
            || string.IsNullOrWhiteSpace(newShowingName))
        {
            throw new ArgumentException("Map and showing names are required.");
        }

        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                UPDATE lua_import_config
                SET ShowingName = $newName
                WHERE MapName = $map AND ShowingName = $oldName;
                """;
            cmd.Parameters.AddWithValue("$map", mapName);
            cmd.Parameters.AddWithValue("$oldName", oldShowingName);
            cmd.Parameters.AddWithValue("$newName", newShowingName);
            cmd.ExecuteNonQuery();
        }
    }

    public void Delete(string mapName, string showingName)
    {
        if (string.IsNullOrWhiteSpace(mapName) || string.IsNullOrWhiteSpace(showingName))
        {
            return;
        }

        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                DELETE FROM lua_import_config
                WHERE MapName = $map AND ShowingName = $name;
                """;
            cmd.Parameters.AddWithValue("$map", mapName);
            cmd.Parameters.AddWithValue("$name", showingName);
            cmd.ExecuteNonQuery();
        }
    }

    public void ReplaceAll(string mapName, IEnumerable<LuaLibConfigRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (string.IsNullOrWhiteSpace(mapName))
        {
            throw new ArgumentException("MapName is required.", nameof(mapName));
        }

        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var tx = conn.BeginTransaction();
            using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM lua_import_config WHERE MapName = $map;";
                del.Parameters.AddWithValue("$map", mapName);
                del.ExecuteNonQuery();
            }

            var order = 0;
            foreach (var record in records)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = """
                    INSERT INTO lua_import_config (MapName, ShowingName, LibPath, OrderNum, IsEnabled)
                    VALUES ($map, $name, $path, $order, $enabled);
                    """;
                cmd.Parameters.AddWithValue("$map", mapName);
                cmd.Parameters.AddWithValue("$name", record.ShowingName);
                cmd.Parameters.AddWithValue("$path", record.LibPath ?? "");
                cmd.Parameters.AddWithValue("$order", order++);
                cmd.Parameters.AddWithValue("$enabled", record.IsEnabled);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }

    private void EnsureSchema()
    {
        AppDataPaths.EnsureUserDataLayout();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS lua_import_config (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                MapName TEXT NOT NULL,
                ShowingName TEXT NOT NULL,
                LibPath TEXT NOT NULL,
                OrderNum INTEGER NOT NULL,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                UNIQUE(MapName, ShowingName)
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
