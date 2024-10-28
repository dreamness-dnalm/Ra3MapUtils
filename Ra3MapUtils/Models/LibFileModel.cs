using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using UtilLib.mapXmlOperator;

namespace Ra3MapUtils.Models;

public partial class LibFileModel: ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _filePath;
    [ObservableProperty] private string _fileType;
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isIncluded = true;
    [ObservableProperty] private bool _runOnce = true;
    [ObservableProperty] private int _orderNum = 99;
    [ObservableProperty] private ObservableCollection<LibFileModel> _children = new ObservableCollection<LibFileModel>();

    private string _libPath;
    private LibFileModel _parent;
    // [ObservableProperty] private LibFileModel _parent;
    
    public static LibFileModel Load(string libPath)
    {
        var rootModel = LoadFromPath(libPath, Directory.GetParent(libPath).FullName, null);
        var metaModel = LoadFromMeta(libPath);
        
        // 不管如何, root先套用配置
        rootModel._filePath = "dir";
        rootModel.IsEnabled = metaModel.IsEnabled;
        rootModel.IsIncluded = metaModel.IsIncluded;
        rootModel.RunOnce = metaModel.RunOnce;
        
        // 递归套用配置
        ApplyMeta(rootModel, metaModel);
        
        SaveMeta(libPath, rootModel);
        return rootModel;
    }
    
    private static void ApplyMeta(LibFileModel dirModel, LibFileModel metaDirModel)
    {
        foreach (var dirModelChild in dirModel.Children)
        {
            var fileName = dirModelChild.FileName;
            var fileType = dirModelChild.FileType;
            var metaModelChildColl = metaDirModel.Children.Where(mm => mm.FileName == fileName && mm.FileType == fileType)
                .Take(1);
            if (metaModelChildColl.Count() == 1)
            {
                var metaModelChild = metaModelChildColl.ToList()[0];
                dirModelChild.IsEnabled = metaModelChild.IsEnabled;
                dirModelChild.IsIncluded = metaModelChild.IsIncluded;
                dirModelChild.RunOnce = metaModelChild.RunOnce;

                if (fileType == "dir")
                {
                    ApplyMeta(dirModelChild, metaModelChild);
                }
            }
        }
        dirModel.Children.OrderBy(m => m.OrderNum);
        for (int i = 0; i < dirModel.Children.Count; i++)
        {
            dirModel.Children[i].OrderNum = i;
        }
    }

    private static LibFileModel LoadFromPath(string libPath, string rootParentPath, LibFileModel parent)
    {
        if (!Directory.Exists(libPath))
        {
            throw new Exception("载入库失败, 目标应该是个文件夹: " + libPath);
        }

        var model = new LibFileModel();
        model._fileName = Path.GetFileName(libPath);
        model._filePath = libPath.Replace(rootParentPath, "");
        model._parent = parent;
        model._fileType = "dir";
        model._libPath = libPath;
        
        var subDirectories = Directory.GetDirectories(libPath).ToList();

        subDirectories.ForEach(d =>
        {
            var path = Path.Combine(libPath, d);
            model.Children.Add(LoadFromPath(path, rootParentPath, parent));
        });

        var subFiles = Directory.GetFiles(libPath).Where(f => f.EndsWith(".lua")).ToList();
        
        subFiles.ForEach(f =>
        {
            var subFileModel = new LibFileModel();
            subFileModel._fileType = "lua";
            subFileModel._fileName = Path.GetFileName(f);
            subFileModel._filePath = f.Replace(rootParentPath, "");
            subFileModel._parent = parent;
            subFileModel._libPath = libPath;
            model.Children.Add(subFileModel);
        });

        model.Children.OrderBy(m => m.OrderNum);
        return model;
    }

    public static LibFileModel LoadFromMeta(string libPath)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        if (!File.Exists(filePath))
        {
            return new LibFileModel();
        }
        return JsonConvert.DeserializeObject<LibFileModel>(File.ReadAllText(filePath));
    }
    
    public static void SaveMeta(string libPath, LibFileModel model)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        File.WriteAllText(filePath, JsonConvert.SerializeObject(model, Formatting.Indented));
    }

    public void Delete()
    {
        if (_parent != null)
        {
            var path = Path.Combine(_libPath, _filePath);
            // todo mod
            _parent.Children.Remove(this);
        }
    }

    private (XElement, int) _export()
    {
        // var splits = FilePath.Split("\\").ToList();
        // var path = Path.Combine(_libPath, string.Join("/", splits.Slice(1, splits.Count - 1)));
        var path = Path.Combine(_libPath, FileName);
        if (_fileType == "lua")
        {
            return (MapXmlHelper.MakeScript(FileName,
                new List<string>() { File.ReadAllText(path) },
                IsEnabled,
                IsIncluded,
                RunOnce), OrderNum);
        }
        else if(_fileType == "dir")
        {
            var childScriptList = new List<(XElement, int)>();
            
            var childScriptGroupList = new List<(XElement, int)>();
            
            foreach (var child in Children)
            {
                var t = child._export();
                if (t.Item1 != null)
                {
                    if (child.FileType == "lua")
                    {
                        childScriptList.Add((t.Item1, t.Item2));
                    }else if (child.FileType == "lua")
                    {
                        childScriptGroupList.Add((t.Item1,t.Item2));
                    }
                }

            }

            var group = MapXmlHelper.MakeScriptGroup(
                FileName, 
                childScriptList.OrderBy(t => t.Item2).Select(t => t.Item1).ToList(), 
                childScriptGroupList.OrderBy(t => t.Item2).Select(t => t.Item1).ToList(), 
                IsEnabled, 
                IsIncluded);
            return (group, OrderNum);
        }

        return (null, 99);
    }

    public XElement Export()
    {
        return _export().Item1;
    }
}