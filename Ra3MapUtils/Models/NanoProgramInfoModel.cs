namespace Ra3MapUtils.Models;

public class NanoProgramInfoModel
{
    public string ID { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string Path { get; set; }
    
    public NanoProgramInstallType InstallType { get; set; }
    
    public static NanoProgramInfoModel Of(string nanoProgramDirectory, NanoProgramInstallType installType)
    {
        var filePath = System.IO.Path.Combine(nanoProgramDirectory, "info.json");
        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }
        var json = System.IO.File.ReadAllText(filePath);
        var o = System.Text.Json.JsonSerializer.Deserialize<NanoProgramInfoModel>(json);
        o.Path = nanoProgramDirectory;
        o.InstallType = installType;
        return o;
    }
}

public enum NanoProgramInstallType
{
    Official,
    User,
    Store
}