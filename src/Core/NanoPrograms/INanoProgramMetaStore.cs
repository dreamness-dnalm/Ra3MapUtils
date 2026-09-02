namespace Core.NanoPrograms;

public interface INanoProgramMetaStore
{
    IReadOnlyList<NanoProgramMetaRecord> GetAll();

    void AddOrUpdate(string id, bool isEnabled, bool isWbVisible, int orderNum);

    void DeleteUnused(IReadOnlyCollection<string> usedIds);
}
