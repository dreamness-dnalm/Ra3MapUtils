using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Util;
using Newtonsoft.Json;
using UtilCoreLib.mapScriptHelper;
using UtilLib.utils;

namespace SharedFunctionLib.Models;

public class SimpleLibFileModel
{
    public string FileName;
    public string FilePath;
    public string FileType;
    public bool IsEnabled = true;
    public bool IsIncluded = true;
    public bool RunOnce = true;
    public bool IsEvaluateEachFrame = true;
    public int EvaluationInterval = 1;
    public bool ActiveInEasy = true;
    public bool ActiveInMedium = true;
    public bool ActiveInHard = true;
    public int OrderNum = 99;
    public List<SimpleLibFileModel> Children = new List<SimpleLibFileModel>();

    public string LibPath;
    private SimpleLibFileModel _parent;

    public override string ToString()
    {
        return "SimpleLibFileModel{" +
               "FileName='" + FileName + '\'' +
               ", FilePath='" + FilePath + '\'' +
               ", FileType='" + FileType + '\'' +
               ", IsEnabled=" + IsEnabled +
               ", IsIncluded=" + IsIncluded +
               ", RunOnce=" + RunOnce +
                ", IsEvaluateEachFrame=" + IsEvaluateEachFrame +
               ", EvaluationInterval=" + EvaluationInterval +
                ", ActiveInEasy=" + ActiveInEasy +
               ", ActiveInMedium=" + ActiveInMedium +
                ", ActiveInHard=" + ActiveInHard +
               ", OrderNum=" + OrderNum +
               ", Children=" + Children +
               ", LibPath='" + LibPath + '\'' +
               '}';
    }


    public static SimpleLibFileModel Load(string libPath)
    {
        var rootModel = LoadFromPath(libPath, Directory.GetParent(libPath).FullName, null);
        var metaModel = LoadFromMeta(libPath);
        
        // 不管如何, root先套用配置
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
        
        // 递归套用配置
        ApplyMeta(rootModel, metaModel);
        
        SaveMeta(libPath, rootModel);
        return rootModel;
    }
    
    private static void ApplyMeta(SimpleLibFileModel dirModel, SimpleLibFileModel metaDirModel)
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
                dirModelChild.OrderNum = metaModelChild.OrderNum;
                dirModelChild.ActiveInEasy = metaModelChild.ActiveInEasy;
                dirModelChild.ActiveInMedium = metaModelChild.ActiveInMedium;
                dirModelChild.ActiveInHard = metaModelChild.ActiveInHard;
                dirModelChild.EvaluationInterval = metaModelChild.EvaluationInterval;
                dirModelChild.IsEvaluateEachFrame = metaModelChild.IsEvaluateEachFrame;
                
                if (fileType == "dir")
                {
                    ApplyMeta(dirModelChild, metaModelChild);
                }
            }
        }
        
        var sorted = dirModel.Children.OrderBy(m => m.OrderNum).ToList();
        for (var i = 0; i < dirModel.Children.Count; i++)
        {
            dirModel.Children[i] = sorted[i];
        }
        
        for (int i = 0; i < dirModel.Children.Count; i++)
        {
            dirModel.Children[i].OrderNum = i;
        }
    }

    private static SimpleLibFileModel LoadFromPath(string libPath, string rootParentPath, SimpleLibFileModel parent)
    {
        if (!Directory.Exists(libPath))
        {
            throw new Exception("载入库失败, 目标应该是个文件夹: " + libPath);
        }

        var model = new SimpleLibFileModel();
        model.FileName = Path.GetFileName(libPath);
        model.FilePath = libPath.Replace(rootParentPath, "");
        model._parent = parent;
        model.FileType = "dir";
        model.LibPath = libPath;
        
        var subDirectories = Directory.GetDirectories(libPath).ToList();

        subDirectories.ForEach(d =>
        {
            var path = Path.Combine(libPath, d);
            model.Children.Add(LoadFromPath(path, rootParentPath, model));
        });

        var subFiles = Directory.GetFiles(libPath).Where(f => f.EndsWith(".lua")).ToList();
        
        subFiles.ForEach(f =>
        {
            var subFileModel = new SimpleLibFileModel();
            subFileModel.FileType = "lua";
            subFileModel.FileName = Path.GetFileName(f);
            subFileModel.FilePath = f.Replace(rootParentPath, "");
            subFileModel._parent = model;
            subFileModel.LibPath = libPath;
            model.Children.Add(subFileModel);
        });

        model.Children.OrderBy(m => m.OrderNum);
        return model;
    }

    public static SimpleLibFileModel LoadFromMeta(string libPath)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        if (!File.Exists(filePath))
        {
            return new SimpleLibFileModel();
        }
        return JsonConvert.DeserializeObject<SimpleLibFileModel>(File.ReadAllText(filePath));
    }
    
    public static void SaveMeta(string libPath, SimpleLibFileModel model)
    {
        var filePath = Path.Combine(libPath, ".lib_meta.json");
        File.WriteAllText(filePath, JsonConvert.SerializeObject(model, Formatting.Indented));
    }
    
    public Tuple2 Translate(MapDataContext context)
    {
        if (FileType == "lua")
        {
            
            var path = Path.Combine(LibPath, FileName);
            Logger.WriteLog("a lua: " + path);
            return new Tuple2(MapScriptHelper.MakeScript(
                context, 
                FileName, 
                new List<string>(){File.ReadAllText(path)},
                IsEnabled,
                IsIncluded,
                RunOnce), OrderNum);
        }
        else if (FileType == "dir")
        {
            Logger.WriteLog("a dir");
            var childScriptList = new List<(Script, int)>();
            var childScriptGroupList = new List<(ScriptGroup, int)>();

            foreach (var child in Children)
            {
                Logger.WriteLog("a child of dir");
                var t = child.Translate(context);
                if (t.Item1 != null)
                {
                    if (child.FileType == "lua")
                    {
                        childScriptList.Add(((Script)t.Item1, t.Item2));
                    }else if(child.FileType == "dir")
                    {
                        childScriptGroupList.Add(((ScriptGroup)t.Item1, t.Item2));
                    }
                }
            }

            return new Tuple2(MapScriptHelper.MakeScriptGroup(
                    context,
                    FileName,
                    childScriptList.OrderBy(t => t.Item2).Select(t => t.Item1).ToList(),
                    childScriptGroupList.OrderBy(t => t.Item2).Select(t => t.Item1).ToList(),
                    IsEnabled,
                    IsIncluded
                )
                , OrderNum);

        }

        return new Tuple2(null, 99);
    }
    
    public class Tuple2
    {
        public object Item1;
        public int Item2;
        
        public Tuple2(object item1, int item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    
}