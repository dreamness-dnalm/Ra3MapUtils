using System.Reflection;
using MapCoreLib.Core;
using MapCoreLib.Core.Scripts;
using MapCoreLib.Core.Util;

namespace test_field;

public class Test_plugin
{
    public static void Main()
    {
        var mapName = "NewMap19";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);
        
        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));

        ScriptHandler.runScript(mapName, "RELOAD");
        // ScriptHandler.doRunScript(ra3Map, "RELOAD");
        
        // var basePath = "H:\\workspace\\dreamness_ra3_tools\\SharedFunctionLib\\bin\\Debug";
        //
        // Assembly assembly = Assembly.LoadFrom(Path.Combine(basePath, "SharedFunctionLib.dll"));
        // var type = assembly.GetType("SharedFunctionLib.Business.LuaImporterBusiness");
        //
        //
        //
        // MethodInfo method = type.GetMethod("ImportActiveMapLua", BindingFlags.Static | BindingFlags.Public);
        //
        //
        // method.Invoke(null, new object[] { context });
        
        // ra3Map.doSaveMap(ra3Map.mapPath);

    }

}