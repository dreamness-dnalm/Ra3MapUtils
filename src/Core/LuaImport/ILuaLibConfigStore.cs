namespace Core.LuaImport;

public interface ILuaLibConfigStore
{
    IReadOnlyList<LuaLibConfigRecord> Load(string mapName);

    void Save(LuaLibConfigRecord record);

    void Rename(string mapName, string oldShowingName, string newShowingName);

    void Delete(string mapName, string showingName);

    void ReplaceAll(string mapName, IEnumerable<LuaLibConfigRecord> records);
}
