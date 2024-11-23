using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

public interface INewWorldBuilderPluginService
{
    public Task<bool> CheckAndInstallNewPluginsAvailable(NewWorldBuilderModel newWorldBuilderModel, string newWorldBuilderPath);
    
    public void OpenPluginFolder(string newWorldBuilderPath);
    
}