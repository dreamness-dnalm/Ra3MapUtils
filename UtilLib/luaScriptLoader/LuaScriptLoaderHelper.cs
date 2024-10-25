using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using UtilLib.luaScriptLoader.entity;
using UtilLib.mapFileHelper;
using Script = UtilLib.luaScriptLoader.entity.Script;

namespace UtilLib.luaScriptLoader
{
    public class LuaScriptLoaderHelper
    {
        public static void LoadLuaScript(string configPath)
        {
            var luaScriptLoaderConfig = LoadConfig(configPath);

            var (mapPath, mapName)  = MapFileHelper.TranslateMapPath(luaScriptLoaderConfig.MapPath);
            if (!MapFileHelper.IsMap(mapPath))
            {
                throw new Exception("Map path is not a map: " + mapPath);
            }

            var ra3Map = new Ra3Map(Path.Combine(mapPath, mapName + ".map"));
            ra3Map.parse();
            var mapScriptGroupListener = new MapScriptGroupListener();
            ra3Map.visit(mapScriptGroupListener);
            var scriptListObj = mapScriptGroupListener.OutScriptLists[0];
            
            luaScriptLoaderConfig.Children.ForEach(scriptUnit =>
            {
                if(scriptUnit is Script scriptConfig)
                {
                    List<MapCoreLib.Core.Asset.Script> scripts = scriptListObj.scripts;
                    if (scripts.Select(i => i.Name == scriptConfig.Name).ToList().Count == 0)
                    {
                        var newScript = new MapCoreLib.Core.Asset.Script();
                        newScript.Name = scriptConfig.Name;
                        
                        var trueOrCondition = new OrCondition();
                        var scriptCondition = new ScriptCondition();
                        scriptCondition.scriptContent = new ScriptContent();
                        scriptCondition.scriptContent.contentName = "CONDITION_TRUE";
                        scriptCondition.scriptContent.assetPropertyType = AssetPropertyType.stringType;
                        trueOrCondition.conditions.Add(scriptCondition);
                        newScript.scriptOrConditions.Add(trueOrCondition);
                        
                        scriptConfig.LuaScripts.ForEach(s =>
                        {
                            var action = new ScriptAction();
                            action.contentName = "DEBUG_MESSAGE_BOX";
                            var scriptArgument = new ScriptArgument();
                            scriptArgument.argumentType = 10;
                            scriptArgument.stringValue = "#!ra3luabridge\r\n" + "local a  = 1";

                            newScript.ScriptActionOnTrue.Add(action);
                        });
                        
                        // newScript.ScriptContent = scriptConfig.ScriptContent;
                        // scripts.Add(newScript);
                    }
                    
                }else if(scriptUnit is Folder folderConfig)
                {
                    var scriptGroups = scriptListObj.scriptGroups;
                    if (scriptGroups.Select(i => i.Name == folderConfig.Name).ToList().Count == 0)
                    {
                        var newScriptGroup = new ScriptGroup();
                        newScriptGroup.Name = folderConfig.Name;
                        scriptGroups.Add(newScriptGroup);
                    }
                }
            });
            scriptListObj.registerSelf(ra3Map.getContext());

            // ApplyFolder(luaScriptLoaderConfig, "/");
            
            ra3Map.doSaveMap(ra3Map.mapPath);
        }
        


        public static void ApplyFolder(Folder folder, string parentPath)
        {
            
        }
        
        
        private static luaScriptLoader.entity.LuaScriptLoader LoadConfig(string configPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LuaScriptLoader));

            // 打开文件并反序列化为对象
            using (FileStream fileStream = new FileStream(configPath, FileMode.Open))
            {
                LuaScriptLoader ret = (LuaScriptLoader)serializer.Deserialize(fileStream);
                return ret;
            }
        }

        public static void Main()
        {
            // var config = LoadConfig("H:\\workspace\\dreamness_ra3_tools\\test.xml");
            // Console.WriteLine(config);
            LoadLuaScript("H:\\workspace\\dreamness_ra3_tools\\test.xml");
        }
    }
}