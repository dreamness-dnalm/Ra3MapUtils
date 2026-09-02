using Core.Paths;
using Microsoft.Data.Sqlite;

namespace Core.NanoPrograms;

public sealed class SqliteNanoProgramMetaStore : INanoProgramMetaStore
{
    private readonly string _dbPath;
    private readonly object _gate = new();

    public SqliteNanoProgramMetaStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppDataPaths.V2SqliteDbPath;
    }

    public IReadOnlyList<NanoProgramMetaRecord> GetAll()
    {
        lock (_gate)
        {
            EnsureSchema();
            var list = new List<NanoProgramMetaRecord>();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT ID, IsEnabled, IsWbVisible, OrderNum
                FROM nano_program_meta
                ORDER BY OrderNum;
                """;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new NanoProgramMetaRecord
                {
                    Id = reader.GetString(0),
                    IsEnabled = reader.GetInt64(1) != 0,
                    IsWbVisible = reader.GetInt64(2) != 0,
                    OrderNum = (int)reader.GetInt64(3),
                });
            }

            return list;
        }
    }

    public void AddOrUpdate(string id, bool isEnabled, bool isWbVisible, int orderNum)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("id is required", nameof(id));
        }

        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO nano_program_meta (ID, IsEnabled, IsWbVisible, OrderNum)
                VALUES ($id, $enabled, $visible, $order)
                ON CONFLICT(ID) DO UPDATE SET
                    IsEnabled = excluded.IsEnabled,
                    IsWbVisible = excluded.IsWbVisible,
                    OrderNum = excluded.OrderNum;
                """;
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$enabled", isEnabled ? 1 : 0);
            cmd.Parameters.AddWithValue("$visible", isWbVisible ? 1 : 0);
            cmd.Parameters.AddWithValue("$order", orderNum);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteUnused(IReadOnlyCollection<string> usedIds)
    {
        lock (_gate)
        {
            EnsureSchema();
            using var conn = Open();
            var existing = GetAllUnlocked(conn);
            foreach (var record in existing)
            {
                if (usedIds.Contains(record.Id))
                {
                    continue;
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM nano_program_meta WHERE ID = $id;";
                cmd.Parameters.AddWithValue("$id", record.Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void EnsureSchema()
    {
        AppDataPaths.EnsureUserDataLayout();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS nano_program_meta
            (
                ID TEXT PRIMARY KEY,
                IsEnabled INTEGER NOT NULL DEFAULT 1,
                IsWbVisible INTEGER NOT NULL DEFAULT 1,
                OrderNum INTEGER NOT NULL DEFAULT -1
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

    private static List<NanoProgramMetaRecord> GetAllUnlocked(SqliteConnection conn)
    {
        var list = new List<NanoProgramMetaRecord>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT ID, IsEnabled, IsWbVisible, OrderNum
            FROM nano_program_meta
            ORDER BY OrderNum;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new NanoProgramMetaRecord
            {
                Id = reader.GetString(0),
                IsEnabled = reader.GetInt64(1) != 0,
                IsWbVisible = reader.GetInt64(2) != 0,
                OrderNum = (int)reader.GetInt64(3),
            });
        }

        return list;
    }
}
