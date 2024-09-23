namespace UtilLib.luaScriptLoader.entity;
[Serializable]
public class Folder: ScriptUnit
{
    public List<ScriptUnit> Children { get; set; }
}