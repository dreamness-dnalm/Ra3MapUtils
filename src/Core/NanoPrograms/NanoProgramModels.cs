using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.NanoPrograms;

public enum NanoProgramInstallType
{
    Official = 0,
    User = 1,
    Store = 2,
}

public sealed class NanoProgramInfoModel
{
    public string ID { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Optional English display name (v1 packages may omit).</summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>Optional English description (v1 packages may omit).</summary>
    public string DescriptionEn { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public NanoProgramInstallType InstallType { get; set; }

    public static NanoProgramInfoModel? Of(string nanoProgramDirectory, NanoProgramInstallType installType)
    {
        var filePath = System.IO.Path.Combine(nanoProgramDirectory, "info.json");
        if (!File.Exists(filePath))
        {
            return null;
        }

        var mainCs = System.IO.Path.Combine(nanoProgramDirectory, "Main.cs");
        if (!File.Exists(mainCs))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        var model = JsonSerializer.Deserialize<NanoProgramInfoModel>(json, JsonOptions);
        if (model == null || string.IsNullOrWhiteSpace(model.ID))
        {
            return null;
        }

        model.Path = nanoProgramDirectory;
        model.InstallType = installType;
        return model;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

public sealed class NanoProgramModel
{
    public NanoProgramInfoModel Info { get; set; } = new();

    public bool IsEnabled { get; set; } = true;

    public bool IsWbVisible { get; set; } = true;

    public int Order { get; set; } = -1;
}

public sealed class NanoProgramMetaRecord
{
    public string Id { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public bool IsWbVisible { get; set; } = true;

    public int OrderNum { get; set; } = -1;
}

public sealed class NanoProgramExecutionResult
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public object? ReturnValue { get; set; }

    public string? Output { get; set; }

    public string? Exception { get; set; }

    [JsonIgnore]
    public Exception? ExceptionObject { get; set; }
}
