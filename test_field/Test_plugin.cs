using System.Reflection;
using MapCoreLib.Core;
using MapCoreLib.Core.Util;

namespace test_field;

public class Test_plugin
{
    public static void Main()
    {
        var mapName = "NewMap17";
        var mapDir = Path.Combine(PathUtil.RA3MapFolder, mapName);

        var ra3Map = new Ra3Map(Path.Combine(mapDir, mapName + ".map"));
        ra3Map.parse();
        
        var context = ra3Map.getContext();
        Assembly assembly = Assembly.LoadFrom(@"H:\workspace\dreamness_ra3_tools\UtilCoreLib\bin\Debug\net45\UtilCoreLib.dll");
        var type = assembly.GetType("UtilCoreLib.mapScriptHelper.MapScriptHelper");
        
        
        
        MethodInfo method = type.GetMethod("test2", BindingFlags.Static | BindingFlags.Public);
        
        
        method.Invoke(null, new object[] { context });
        
        ra3Map.doSaveMap(ra3Map.mapPath);

    }

}