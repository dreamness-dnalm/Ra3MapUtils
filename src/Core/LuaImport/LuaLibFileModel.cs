using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.LuaImport;

/// <summary>
/// Directory/Lua tree for a library folder, with optional .lib_meta.json overlays.
/// </summary>
public sealed class LuaLibFileModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string FileName { get; set; } = "";

    public string FilePath { get; set; } = "";

    public string FileType { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public bool IsIncluded { get; set; } = true;

    public bool RunOnce { get; set; } = true;

    public bool IsEvaluateEachFrame { get; set; } = true;

    public int EvaluationInterval { get; set; } = 1;

    public bool ActiveInEasy { get; set; } = true;

    public bool ActiveInMedium { get; set; } = true;

    public bool ActiveInHard { get; set; } = true;

    public int OrderNum { get; set; } = 99;

    public List<LuaLibFileModel> Children { get; set; } = new();

    public string LibPath { get; set; } = "";

    public static LuaLibFileModel Load(string libPath)
    {
        var parent = Directory.GetParent(libPath)
                     ?? throw new DirectoryNotFoundException("Cannot resolve parent for lib path: " + libPath);
        var rootModel = LoadFromPath(libPath, parent.FullName);
        var metaModel = LoadFromMeta(libPath);

        rootModel.FileType = "dir";
        rootModel.FilePath = libPath;
        rootModel.IsEnabled = metaModel.IsEnabled;
        rootModel.IsIncluded = metaModel.IsIncluded;
        rootModel.ActiveInEasy = metaModel.ActiveInEasy;
        rootModel.ActiveInMedium = metaModel.ActiveInMedium;
        rootModel.ActiveInHard = metaModel.ActiveInHard;
        rootModel.EvaluationInterval = metaModel.EvaluationInterval;
        rootModel.IsEvaluateEachFrame = metaModel.IsEvaluateEachFrame;
        rootModel.RunOnce = metaModel.RunOnce;

        ApplyMeta(rootModel, metaModel);
        SaveMeta(libPath, rootModel);
        return rootModel;
    }

    private static void ApplyMeta(LuaLibFileModel dirModel, LuaLibFileModel metaDirModel)
    {
        foreach (var dirModelChild in dirModel.Children)
        {
            var metaModelChild = metaDirModel.Children
                .FirstOrDefault(mm => mm.FileName == dirModelChild.FileName && mm.FileType == dirModelChild.FileType);
            if (metaModelChild is null)
            {
                continue;
            }

            dirModelChild.IsEnabled = metaModelChild.IsEnabled;
            dirModelChild.IsIncluded = metaModelChild.IsIncluded;
            dirModelChild.RunOnce = metaModelChild.RunOnce;
            dirModelChild.OrderNum = metaModelChild.OrderNum;
            dirModelChild.ActiveInEasy = metaModelChild.ActiveInEasy;
            dirModelChild.ActiveInMedium = metaModelChild.ActiveInMedium;
            dirModelChild.ActiveInHard = metaModelChild.ActiveInHard;
            dirModelChild.EvaluationInterval = metaModelChild.EvaluationInterval;
            dirModelChild.IsEvaluateEachFrame = metaModelChild.IsEvaluateEachFrame;

            if (dirModelChild.FileType == "dir")
            {
                ApplyMeta(dirModelChild, metaModelChild);
            }
        }

        var sorted = dirModel.Children.OrderBy(m => m.OrderNum).ToList();
        for (var i = 0; i < dirModel.Children.Count; i++)
        {
            dirModel.Children[i] = sorted[i];
            dirModel.Children[i].OrderNum = i;
        }
    }

    private static LuaLibFileModel LoadFromPath(string libPath, string rootParentPath)
    {
        if (!Directory.Exists(libPath))
        {
            throw new Exception("载入库失败, 目标应该是个文件夹: " + libPath);
        }

        var model = new LuaLibFileModel
        {
            FileName = Path.GetFileName(libPath),
            FilePath = libPath.Replace(rootParentPath, ""),
            FileType = "dir",
            LibPath = libPath,
        };

        foreach (var d in Directory.GetDirectories(libPath))
        {
            model.Children.Add(LoadFromPath(d, rootParentPath));
        }

        foreach (var f in Directory.GetFiles(libPath).Where(x => x.EndsWith(".lua", StringComparison.OrdinalIgnoreCase)))
        {
            model.Children.Add(new LuaLibFileModel
            {
                FileType = "lua",
                FileName = Path.GetFileName(f),
                FilePath = f.Replace(rootParentPath, ""),
                LibPath = libPath,
            });
        }

        return model;
    }

    public static LuaLibFileModel LoadFromMeta(string libPath)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        if (!File.Exists(filePath))
        {
            return new LuaLibFileModel();
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<LuaLibFileModel>(json, JsonOptions) ?? new LuaLibFileModel();
    }

    public static void SaveMeta(string libPath, LuaLibFileModel model)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(model, JsonOptions));
    }
}
