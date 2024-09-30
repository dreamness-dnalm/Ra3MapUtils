using System;
using System.IO;
using System.Linq;
using MapCoreLib.Core;
using MapCoreLib.Core.Asset;
using MapCoreLib.Core.Util;

namespace UtilLib.luaScriptLoader
{

    public class test2
    {
        public static void Main()
        {
            string mapName = "AmazeTransferVehicle";


            // Ra3Map ra3Map = new Ra3Map(Path.Combine(PathUtil.RA3MapFolder, mapName, mapName + ".map"));
            Ra3Map ra3Map = new Ra3Map(Path.Combine("D:\\workspace\\mia\\Ra3MapUtils\\test_map", mapName, mapName + ".map"));
            ra3Map.parse();

            var visitImpl = ra3Map.mapVisitImpl;



            var context = ra3Map.getContext();

            var mapScriptGroupListener = new MapScriptGroupListener();
            ra3Map.visit(mapScriptGroupListener);
            var outScriptLists = mapScriptGroupListener.OutScriptLists;

            Console.WriteLine(outScriptLists.Count());


            var sl = outScriptLists[1];
            // sl.scripts
            

            // var scriptList = context.getAsset<ScriptList>("..");

            // var scriptList = new ScriptList();
            // scriptList.scriptGroups.Add(new ScriptGroup());
            // scriptList.scriptGroups[0].scripts.Add(new Script());
            //
            //
            // ra3Map.doSaveMap(ra3Map.mapPath);
        }
    }
}