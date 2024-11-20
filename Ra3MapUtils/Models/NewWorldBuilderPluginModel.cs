using System.IO;
using System.Windows.Documents;
using Newtonsoft.Json;

namespace Ra3MapUtils.Models;

public class NewWorldBuilderPluginModel
{
    public string PluginName { get; set; }
    public string PluginVersion { get; set; }
    public Dictionary<string, string> RequireFileDictionary;

    public static async Task<List<NewWorldBuilderPluginModel>> LoadPluginModelsAsync(string scriptsPath)
    {
        if (!Directory.Exists(scriptsPath))
        {
            return new List<NewWorldBuilderPluginModel>();
        }

        return Directory.GetDirectories(scriptsPath).ToList()
            .SelectMany(p =>
            {
                var pluginName = Path.GetFileName(p);
                var pluginMetaFilePath = Path.Combine(p, "plugin_meta.json");
                if (!File.Exists(pluginMetaFilePath))
                {
                    return new List<NewWorldBuilderPluginModel>();
                }

                var newWorldBuilderPluginModel = LoadPluginMeta(pluginMetaFilePath);
                if (newWorldBuilderPluginModel == null)
                {
                    return new List<NewWorldBuilderPluginModel>();
                }
                else
                {
                    return new List<NewWorldBuilderPluginModel> { newWorldBuilderPluginModel };
                }
            }).ToList();
    }
    

    public static NewWorldBuilderPluginModel? LoadPluginMeta(string metaPath)
    {
        if (!File.Exists(metaPath))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<NewWorldBuilderPluginModel>(File.ReadAllText(metaPath));
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static void main()
    {
        var path =
            "H:\\Program Files (x86)\\Red Alert 3(Incomplete)\\CoronaLauncher_Test_3.11.9028.39760\\CoronaResources\\NewWorldBuilder\\data\\scripts";
        var newWorldBuilderPluginModels = LoadPluginModelsAsync(path).Result;
    }
}