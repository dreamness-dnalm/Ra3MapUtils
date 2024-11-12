using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Ra3MapUtils.Models;

public partial class MapFileModel: ObservableObject
{
    [ObservableProperty] private string _comment = "";

    public static MapFileModel Load(string mapDirPath)
    {
        var mapModel = LoadFromPath(mapDirPath);
        var metaMapModel = LoadFromMeta(mapDirPath);
        
        ApplyMeta(mapModel, metaMapModel);
        
        SaveMeta(mapDirPath, mapModel);
        return mapModel;
    }
    
    private static MapFileModel LoadFromPath(string mapDirPath)
    {
        return new MapFileModel();
    }
    
    private static MapFileModel LoadFromMeta(string mapDirPath)
    {
        var filePath = Path.Combine(mapDirPath, "map_meta.json");
        if (!File.Exists(filePath))
        {
            return new MapFileModel();
        }
        return JsonConvert.DeserializeObject<MapFileModel>(File.ReadAllText(filePath));
    }

    private static void ApplyMeta(MapFileModel mapModel, MapFileModel metaMapModel)
    {
        mapModel.Comment = metaMapModel.Comment;
    }
    
    public static void SaveMeta(string mapDirPath, MapFileModel mapModel)
    {
        var filePath = Path.Combine(mapDirPath, "map_meta.json");
        File.WriteAllText(filePath, JsonConvert.SerializeObject(mapModel, Formatting.Indented));
    }
}