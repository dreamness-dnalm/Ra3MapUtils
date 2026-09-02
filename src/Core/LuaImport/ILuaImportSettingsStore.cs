namespace Core.LuaImport;

public interface ILuaImportSettingsStore
{
    /// <summary>Default is 100 when unset.</summary>
    int GetRedundancyFactor();

    void SetRedundancyFactor(int factor);

    string? GetActiveMapName();

    string? GetActiveMapFilePath();

    void SetActiveMap(string? mapName, string? mapFilePath);

    void ClearActiveMap();
}
