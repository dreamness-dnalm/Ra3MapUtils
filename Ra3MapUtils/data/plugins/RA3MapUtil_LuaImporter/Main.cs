using System;
using System.Collections.Generic;
using System.Collections;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Diagnostics;
namespace MapCoreLib.Core.Scripts.ScriptFile
{
    public class RA3MapUtil_LuaImporter : ScriptInterface
    {

        public void Apply(MapDataContext context)
        {
            try
            {
                IList models = ExternalFuncHelper.LoadLuaLibConfigModels();
                var scriptList = context.getAsset<PlayerScriptsList>("PlayerScriptsList").scriptLists[0];
                string lastScriptGroupName = null;
                for(int i = 0; i < models.Count; i ++)
                {
                    object model = models[i];
                    string libPath = AssemblyHelper.GetObjectField<string>(model, "LibPath");
                    object fileModel = ExternalFuncHelper.LoadSimpleFileModel(libPath);
                    // bool isEnabled = AssemblyHelper.GetObjectField<int>(model, "IsEnabled") > 0;
                    bool isEnabled = true;
                    ExternalFuncHelper.WriteLog(fileModel.ToString());

                    var scriptGroup = Translate(fileModel, context).Item1;
                    if (scriptGroup != null)
                    {
                        if(isEnabled)
                        {
                            ScriptHelper.AddScriptGroup(context, scriptList, (ScriptGroup)scriptGroup, lastScriptGroupName);                            
                        }
                        else
                        {
                            // TODO 改变顺序
                        }
                        lastScriptGroupName = Path.GetFileName(libPath);
                    }
                }
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);

                string path = "Exception.txt";

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine(st.ToString());
                    writer.WriteLine("line num: " + st.GetFrame(0).GetFileLineNumber());
                    
                }
                ExternalFuncHelper.WriteLog("Exception: " + ex.Message);
                ExternalFuncHelper.WriteLog("Stack Trace: " + ex.StackTrace);
                ExternalFuncHelper.WriteLog("Inner Exception: " + ex.InnerException.Message);

                throw ex;
            }

        }


        public Tuple2 Translate(object model, MapDataContext context)
        {
            string libPath = AssemblyHelper.GetObjectField<string>(model, "LibPath");
            string fileName = AssemblyHelper.GetObjectField<string>(model, "FileName");  
            // string filePath = AssemblyHelper.GetObjectField<string>(model, "FilePath");  
            string fileType = AssemblyHelper.GetObjectField<string>(model, "FileType");  
            bool isEnabled = AssemblyHelper.GetObjectField<bool>(model, "IsEnabled");  
            bool isIncluded = AssemblyHelper.GetObjectField<bool>(model, "IsIncluded"); 
            bool runOnce = AssemblyHelper.GetObjectField<bool>(model, "RunOnce"); 
            int orderNum = AssemblyHelper.GetObjectField<int>(model, "OrderNum");  
            
            bool activeInEasy = AssemblyHelper.GetObjectField<bool>(model, "ActiveInEasy");
            bool activeInMedium = AssemblyHelper.GetObjectField<bool>(model, "ActiveInMedium");
            bool activeInHard = AssemblyHelper.GetObjectField<bool>(model, "ActiveInHard");
            
            int evaluationInterval = AssemblyHelper.GetObjectField<int>(model, "EvaluationInterval");
            bool isEvaluateEachFrame = AssemblyHelper.GetObjectField<bool>(model, "IsEvaluateEachFrame");
            
            IList children = AssemblyHelper.GetObjectField<IList>(model, "Children");

            if (fileType == "lua")
            {
                var path = Path.Combine(libPath, fileName);
                return new Tuple2(ScriptHelper.MakeScript(
                    context,
                    fileName,
                    new List<string>() { File.ReadAllText(path) },
                    isEnabled,
                    isIncluded,
                    runOnce,
                    isEvaluateEachFrame? -1: evaluationInterval,
                    activeInEasy,
                    activeInMedium,
                    activeInHard
                    ), orderNum);
            }
            else if (fileType == "dir")
            {
                var childScripts = new List<Tuple2>();
                var childScriptGroups = new List<Tuple2>();

                foreach (var child in children)
                {
                    var t = Translate(child, context);
                    if (t.Item1 != null)
                    {
                        string childFileType = AssemblyHelper.GetObjectField<string>(child, "FileType");
                        if (childFileType == "lua")
                        {
                            childScripts.Add(t);
                        }
                        else if (childFileType == "dir")
                        {
                            childScriptGroups.Add(t);
                        }
                    }
                }

                return new Tuple2(ScriptHelper.MakeScriptGroup(
                        context,
                        fileName,
                        childScripts.OrderBy(t => t.Item2).Select(t => (Script)t.Item1).ToList(),
                        childScriptGroups.OrderBy(t => t.Item2).Select(t => (ScriptGroup)t.Item1).ToList(),
                        isEnabled,
                        isIncluded), orderNum);

            }
            ExternalFuncHelper.WriteLog("Translate would return (null, 99)!");
            return new Tuple2(null, 99);
        }
    }

    public static class AssemblyHelper
    {
        public static object InvokeStaticMethod(Assembly assembly, string className, string methodName, params object[] parameters)
        {
            var type = assembly.GetType(className);
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            return method.Invoke(null, parameters);
        }

        public static T GetStaticProperty<T>(Assembly assembly, string className, string propertyName)
        {
            var type = assembly.GetType(className);
            return (T)type.GetProperty(propertyName).GetValue(null);
        }

        public static T GetObjectProperty<T>(object obj, string propertyName)
        {
            return (T)obj.GetType().GetProperty(propertyName).GetValue(obj);
        }

        public static T GetObjectField<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName).GetValue(obj);
        }
    }

    public static class ExternalFuncHelper
    {

        private static Assembly sharedFunctionLibAssembly = Assembly.LoadFrom("bin\\SharedFunctionLib.dll");
        private static Assembly utilCoreLibAssembly = Assembly.LoadFrom("bin\\UtilCoreLib.dll");

        public static void WriteLog(string log)
        {
            if(log == null)
            {
                log = "null-log";
            }
            AssemblyHelper.InvokeStaticMethod(utilCoreLibAssembly, "UtilLib.utils.Logger", "WriteLog", log);
        }
    
        public static IList LoadLuaLibConfigModels()
        {
            return (IList)AssemblyHelper.InvokeStaticMethod(sharedFunctionLibAssembly, "SharedFunctionLib.Business.LuaImporterBusiness", "LoadLuaLibConfigModels");
        }

        public static object LoadSimpleFileModel(string libPath)
        {
            return AssemblyHelper.InvokeStaticMethod(sharedFunctionLibAssembly, "SharedFunctionLib.Models.SimpleLibFileModel", "Load", libPath);
        }

        public static int GetLuaRedundancyFactor()
        {
            return AssemblyHelper.GetStaticProperty<int>(sharedFunctionLibAssembly, "SharedFunctionLib.Business.LuaImporterBusiness", "LuaRedundancyFactor");
        }
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

        public override string ToString()
        {
            return "Tuple2{Item1: " + Item1 + ", Item2: " + Item2 + "}";
        }
    }

    public static class ScriptHelper
    {
        public static void AddScriptGroup(MapDataContext context, ScriptList scriptList, ScriptGroup scriptGroup, string behindScriptGroupName)
        {
            RemoveScriptGroup(scriptList, scriptGroup.Name);

            scriptList.scriptGroups.Add(scriptGroup);

            scriptGroup.registerSelf(context);
        }

        public static void RemoveScriptGroup(ScriptList scriptList, string scriptGroupName)
        {
            scriptList.scriptGroups.Where(i => i.Name == scriptGroupName).ToList()
                .ForEach(i => scriptList.scriptGroups.Remove(i));
        }

        public static List<OrCondition> MakeConditionTrue(MapDataContext context)
        {
            return new List<OrCondition>()
            {
                new OrCondition()
                {
                    conditions = new List<ScriptCondition>()
                    {
                        ScriptCondition.of(context, "CONDITION_TRUE")
                    }
                }
            };
        }


        public static Script MakeScript(MapDataContext context, string name, List<string> luaContents, bool isEnable, bool isInclude, bool runOnce, int evaluationInterval, bool activeInEasy, bool activeInMedium, bool activeInHard)
        {
            if (!isInclude)
            {
                return null;
            }
            var script = new Script();
            script.Name = name;
            script.scriptOrConditions = MakeConditionTrue(context);
            script.ScriptActionOnTrue = new List<ScriptAction>();
            script.isActive = isEnable;
            script.DeactivateUponSuccess = runOnce;
            if(evaluationInterval > 0)
            {
                script.EvaluationInterval = evaluationInterval;
            }

            script.ActiveInEasy = activeInEasy;
            script.ActiveInMedium = activeInMedium;
            script.ActiveInHard = activeInHard;


            foreach (var luaContent in luaContents)
            {
                var content = "#!ra3luabridge\r\n" + luaContent;
                // 可能是库的bug, 原因不明, 会吞掉最后的很多字符, 内容长度越长, 吞的越多
                // 因此在后面附加无关的内容用于占位
                content += "\r\n";
                for(int i = 0; i < ExternalFuncHelper.GetLuaRedundancyFactor(); i ++)
                {
                    content += "-- end of script, please ignore this line";
                }
                script.ScriptActionOnTrue.Add(ScriptAction.of(context, "DEBUG_MESSAGE_BOX", new List<object> { (object)content }));
            }

            return script;
        }
    

        public static ScriptGroup MakeScriptGroup(MapDataContext context, string name, List<Script> subScripts, List<ScriptGroup> subScriptGroups, bool isEnable, bool isInclude)
        {
            if (!isInclude)
            {
                return null;
            }
            var scriptGroup = new ScriptGroup
            {
                Name = name,
                IsActive = isEnable
            };
            foreach (var subScript in subScripts)
            {
                scriptGroup.scripts.Add(subScript);
            }

            foreach (var subScriptGroup in subScriptGroups)
            {
                scriptGroup.scriptGroups.Add(subScriptGroup);
            }

            return scriptGroup;
        }
    }
}