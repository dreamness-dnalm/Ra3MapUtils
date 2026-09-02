namespace Core.LuaImport;

public sealed class LuaLibConfigRecord
{
    public string MapName { get; set; } = "";

    public string ShowingName { get; set; } = "";

    public string LibPath { get; set; } = "";

    public int OrderNum { get; set; }

    public int IsEnabled { get; set; } = 1;
}
