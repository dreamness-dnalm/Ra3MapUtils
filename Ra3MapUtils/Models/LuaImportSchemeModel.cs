using Newtonsoft.Json;

namespace Ra3MapUtils.Models;

public class LuaImportSchemeModel
{
    public const string SchemeType = "ra3maputils.luaImportScheme";

    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonProperty("type")]
    public string Type { get; set; } = SchemeType;

    [JsonProperty("items")]
    public List<LuaImportSchemeItemModel> Items { get; set; } = new();
}

public class LuaImportSchemeItemModel
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;
}

public class LuaImportSchemeOperationResult
{
    public string Map { get; set; } = string.Empty;

    public string MapFilePath { get; set; } = string.Empty;

    public string JsonPath { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public int EnabledItemCount { get; set; }
}
