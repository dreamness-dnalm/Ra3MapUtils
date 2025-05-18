using KnowledgeBaseLib.Models;
using Microsoft.Data.Sqlite;

namespace KnowledgeBaseLib.DAO;

public class KnowledgeBaseDAO: IDisposable
{
    private SqliteConnection _connection;
    
    public KnowledgeBaseDAO(string extensionPath, string dbPath, bool init = false)
    {
        var dllPath = Path.Combine(extensionPath, "simple.dll");
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"The file {dllPath} could not be found.");
        }
        
        var dictPath = Path.Combine(extensionPath, "dict");
        if (!Directory.Exists(dictPath))
        {
            throw new DirectoryNotFoundException($"The directory {dictPath} could not be found.");
        }

        if ((!File.Exists(dbPath)) && (!init))
        {
            throw new FileNotFoundException($"The file {dbPath} could not be found.");
        }
        
        _connection = new SqliteConnection($"Data Source={dbPath};");
        _connection.Open();
        _connection.LoadExtension(extensionPath);
        _connection.EnableExtensions(true);
        
        var command = _connection.CreateCommand();
        command.CommandText = $"SELECT jieba_dict('{dictPath}');";
        command.ExecuteNonQuery();
        
        var createTableCommand = _connection.CreateCommand();
        
        createTableCommand.CommandText = "CREATE VIRTUAL TABLE IF NOT EXISTS  kb using fts5(title, content, tags, data_type, tokenize='simple')";
        createTableCommand.ExecuteNonQuery();
    }
    
    public void AddRecord(KnowledgeBaseRecord record)
    {
        var insertCommand = _connection.CreateCommand();
        insertCommand.CommandText = "INSERT INTO kb (title, content, tags) VALUES (@title, @content, @tags, @type_type)";
        insertCommand.Parameters.AddWithValue("@title", record.Title);
        insertCommand.Parameters.AddWithValue("@content", record.Content);
        insertCommand.Parameters.AddWithValue("@tags", record.Tags);
        insertCommand.ExecuteNonQuery();
    }

    
    
    public List<KnowledgeBaseRecord> Search(string query, string tagQuery, int page, int pageSize)
    {
        var queryCommand = _connection.CreateCommand();
        
        queryCommand.Parameters.AddWithValue("@query", query);
        queryCommand.Parameters.AddWithValue("@pageSize", pageSize);
        queryCommand.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
        
        if (string.IsNullOrWhiteSpace(tagQuery))
        {
            queryCommand.CommandText = "SELECT title, content, tags, data_type FROM kb WHERE content MATCH @query LIMIT @pageSize OFFSET @offset";
        }
        else
        {
            queryCommand.CommandText = "SELECT title, content, tags, data_type FROM kb WHERE (content MATCH @query AND tags MATCH @tagQuery)  LIMIT @pageSize OFFSET @offset";
            queryCommand.Parameters.AddWithValue("@tagQuery", tagQuery);
        }
        
        var reader = queryCommand.ExecuteReader();
        var results = new List<KnowledgeBaseRecord>();
        while (reader.Read())
        {
            var title = reader.GetString(0);
            var content = reader.GetString(1);
            var tags = reader.GetString(2);
            var dataType = reader.GetString(3);
            results.Add(new KnowledgeBaseRecord(title, content, tags, dataType));
        }
        return results;
    }
    
    public void Close()
    {
        _connection.Close();
    }
    
    public void Dispose()
    {
        Close();
        _connection.Dispose();
    }
}