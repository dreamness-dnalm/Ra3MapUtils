using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtilLib.mapFileHelper;

namespace UtilLib.mapstrFileHelper
{
    public static class MapStrFileHelper
    {
        public static Dictionary<string, string> LoadAsDictionary(string mapPath)
        {
            string mapName;
            (mapPath, mapName) = MapFileHelper.TranslateMapPath(mapPath);
            
            var mapStrPath = Path.Combine(mapPath, "map.str");

            var retDict = new Dictionary<string, string>();

            if(!File.Exists(mapStrPath))
            {
                return retDict;
            }

            var lines = File.ReadAllLines(mapStrPath);
            
            var status = 0;
            /* 0: 正在寻找key
               1: 正在寻找value
               2: 正在寻找end
            */
            var currKey = "";
            var currValue = "";
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line == "")
                {
                    continue;
                }
                else if (line == "END")
                {
                    if (status == 2)
                    {
                        retDict.Add(currKey, currValue);
                        status = 0;
                        currKey = "";
                        currValue = "";
                    }
                    else
                    {
                        throw new Exception("格式解析错误: " + mapStrPath + " 行数: " + (i + 1));
                    }
                }
                else if (line.Length >=2 && line.StartsWith("\"") && line.EndsWith("\""))
                {
                    if (status == 1)
                    {
                        currValue = line.Substring(1, line.Length - 2);
                        status = 2;
                    }
                    else
                    {
                        throw new Exception("格式解析错误: " + mapStrPath + " 行数: " + (i + 1));
                    }
                }
                else if(char.IsLetter(line[0]) && char.IsLetter(line[line.Length - 1]))
                {
                    if (status == 0)
                    {
                        currKey = line;
                        status = 1;
                    }
                    else
                    {
                        throw new Exception("格式解析错误: " + mapStrPath + " 行数: " + (i + 1));
                    }
                }
                else
                {
                    throw new Exception("格式解析错误: " + mapStrPath + " 行数: " + (i + 1));
                }
            }

            if (status != 1)
            {
                throw new Exception("格式解析错误: " + mapStrPath + " 行数: " + lines.Length);
            }
            
            return retDict;
        }

        public static void Save(string mapPath, Dictionary<string, string> dataDict)
        {
            string mapName;
            (mapPath, mapName) = MapFileHelper.TranslateMapPath(mapPath);
            
            var mapStrPath = Path.Combine(mapPath, "map.str");
            
            var content = "";
            foreach (var pair in dataDict)
            {
                content += pair.Key + "\n";
                content += "\"" + pair.Value + "\"\n";
                content += "END\n";
                content += "\n";
            }
            
            File.WriteAllText(mapStrPath, content);
            
        }

        public static void SetMapName(string mapPath, string mapInsideName)
        {
            string mapName;
            (_, mapName) = MapFileHelper.TranslateMapPath(mapPath);
            
            var dict = LoadAsDictionary(mapPath);
            
            dict.Keys.Where(i => i.StartsWith("MAP:")).ToList().ForEach(i => dict.Remove(i));
            
            dict.Add("MAP:" + mapName, mapInsideName);
            
            Save(mapPath, dict);
        }
    }
}