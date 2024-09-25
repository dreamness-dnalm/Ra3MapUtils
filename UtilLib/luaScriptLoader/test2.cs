using MapCoreLib.Core;
using MapCoreLib.Core.Asset;

namespace UtilLib.luaScriptLoader;

public class test2
{
    public static void Main()
    {
        string name = "name";
        
        
        var m = new Ra3Map(name);
        m.parse();

        var visitImpl = m.mapVisitImpl;
        
        

        var context = m.getContext();
        m.visit(new MapListener());
        
        
        // var scriptList = context.getAsset<ScriptList>("..");

        var scriptList = new ScriptList();
        scriptList.scriptGroups.Add(new ScriptGroup());
        scriptList.scriptGroups[0].scripts.Add(new Script());


        m.save("", name);
    }
}