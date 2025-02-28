using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Scripts;
using MapCoreLib.Util;

namespace test_field;

public class PlaceThing
{
    public class Main: ScriptInterface
    {
        public void Apply(MapDataContext mapDataContext)
        {
            ObjectsList objectsList = mapDataContext.getAsset<ObjectsList>(Ra3MapConst.ASSET_ObjectsList);
            
            var heightMapData = mapDataContext.getAsset<HeightMapData>(Ra3MapConst.ASSET_HeightMapData);

            var random = new Random();
            for (int i = 0; i < 700; i++)
            {
                objectsList.AddObject(mapDataContext, 
                    "HA_Tree01",
                    new Vec3D(mapDataContext.mapWidth * 10 * random.NextDouble(), mapDataContext.mapHeight * 10 * random.NextDouble()),
                    45);
            }
        }
    }
}