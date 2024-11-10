using System;
using System.IO;
using Newtonsoft.Json;
using UtilLib.models;

namespace UtilLib.mapFileHelper
{
    public static class MapInfoJsonHelper
    {
        public static MapInfoJsonModel ReadMapInfoJson(string mapPath)
        {
            string mapName;
            (mapPath, mapName) = MapFileHelper.TranslateMapPath(mapPath);
            var mapInfoJsonPath = Path.Combine(mapPath, mapName + "_info.json");
            if (!File.Exists(mapInfoJsonPath))
            {
                throw new Exception("Map info json file does not exist: " + mapInfoJsonPath);
            }

            var jsonStr = File.ReadAllText(mapInfoJsonPath);
            return JsonConvert.DeserializeObject<MapInfoJsonModel>(jsonStr);
        }
    }
}