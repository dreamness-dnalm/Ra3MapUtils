namespace Core.NanoPrograms;

public interface INanoProgramService
{
    IReadOnlyList<NanoProgramModel> GetNanoPrograms();

    void SaveMeta(string id, bool isEnabled, bool isWbVisible, int orderNum);

    NanoProgramExecutionResult ExecuteNanoProgram(string id, Dictionary<string, string>? arguments);
}
