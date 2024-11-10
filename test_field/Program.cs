// See https://aka.ms/new-console-template for more information

using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;
using MapCoreLib.Util;

var mapName = "NewMap12";
var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
ra3Map.parse();
var mapDataContext = ra3Map.getContext();

// ScriptHandler.runScript(Path.Combine(mapDir, mapName + ".map"), "RandomAddTrees");
            
ObjectsList objectsList = mapDataContext.getAsset<ObjectsList>(Ra3MapConst.ASSET_ObjectsList);

            
var random = new Random();
for (int i = 0; i < 10; i++)
{
    objectsList.AddObject(mapDataContext, 
        "JapanEmperorsRageEffect_Small",
        new Vec3D(mapDataContext.mapWidth * 10 * random.NextDouble(), mapDataContext.mapHeight * 10 * random.NextDouble()),
        45);
}