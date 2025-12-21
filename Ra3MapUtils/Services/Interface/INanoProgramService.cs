using Dreamness.ScriptExecutor;
using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

public interface INanoProgramService
{
    public List<NanoProgramModel> GetNanoPrograms();
    
    public ScriptExecutionResult ExecuteNanoProgram(string id, Dictionary<string, string> arguments);
    
}