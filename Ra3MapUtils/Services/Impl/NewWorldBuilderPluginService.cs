using System.IO;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.Utils;

namespace Ra3MapUtils.Services.Impl;

public class NewWorldBuilderPluginService: INewWorldBuilderPluginService
{
    
    private string GetScriptsPath(string newWorldBuilderPath)
    {
        return Path.Combine(Path.GetDirectoryName(newWorldBuilderPath), "data", "scripts");
    }

    public async Task<bool> CheckAndInstallNewPluginsAvailable(NewWorldBuilderModel newWorldBuilderModel, string newWorldBuilderPath)
    {
        newWorldBuilderModel.IsPluginsInstallError = false;
        newWorldBuilderModel.IsPluginsInstalled = false;
        newWorldBuilderModel.IsPluginsInstalling = true;

        try
        {


            var installedScriptsPath = GetScriptsPath(newWorldBuilderPath);
            var availableScriptsPath = "data\\plugins";

            var installedPluginModelsTask = NewWorldBuilderPluginModel.LoadPluginModelsAsync(installedScriptsPath);
            var availablePluginModelsTask = NewWorldBuilderPluginModel.LoadPluginModelsAsync(availableScriptsPath);

            var installedPluginDict = newWorldBuilderModelsToDictionary(await installedPluginModelsTask);
            var availablePluginDict = newWorldBuilderModelsToDictionary(await availablePluginModelsTask);

            bool ret = true;


            foreach (var (name, model) in availablePluginDict)
            {

                if (installedPluginDict.ContainsKey(name) &&
                    installedPluginDict[name].PluginVersion == model.PluginVersion)
                {
                    continue;
                }

                var installedPluginPath = Path.Combine(installedScriptsPath, name);
                var availablePluginPath = Path.Combine(availableScriptsPath, name);
                try
                {
                    if (Directory.Exists(installedPluginPath))
                    {
                        Directory.Delete(installedPluginPath, true);
                    }

                    Directory.CreateDirectory(installedPluginPath);

                    foreach (var (sourceFilePath, targetPath) in model.RequireFileDictionary)
                    {
                        var fullSourcePath = sourceFilePath;
                        var fullTargetPath = Path.Combine(installedScriptsPath, name, targetPath);

                        var parentTargetPath = Path.GetDirectoryName(fullTargetPath);
                        if (!Directory.Exists(parentTargetPath))
                        {
                            Directory.CreateDirectory(parentTargetPath);
                        }

                        File.Copy(fullSourcePath, fullTargetPath, true);
                    }

                }
                catch (Exception e)
                {
                    throw e;
                    Directory.Delete(installedPluginPath);
                    ret = false;
                    newWorldBuilderModel.IsPluginsInstallError = true;
                    newWorldBuilderModel.IsPluginsInstalled = false;
                    continue;
                }
            }

            if (ret)
            {
                newWorldBuilderModel.IsPluginsInstalled = true;
            }
            return ret;
        }
        catch (Exception e)
        {
            newWorldBuilderModel.IsPluginsInstallError = true;
            
            return false;
        }
        finally
        {
            newWorldBuilderModel.IsPluginsInstalling = false;
        }
    }

    public void OpenPluginFolder(string newWorldBuilderPath)
    {
        var installedScriptsPath = GetScriptsPath(newWorldBuilderPath);
        ExplorerUtil.OpenExplorer(installedScriptsPath, true);
    }


    private Dictionary<string, NewWorldBuilderPluginModel> newWorldBuilderModelsToDictionary(List<NewWorldBuilderPluginModel> models)
    {
        return models.ToDictionary(m => m.PluginName, m => m);
    }

    public static void main()
    {
        var newWorldBuilderPluginService = new NewWorldBuilderPluginService();

        newWorldBuilderPluginService.CheckAndInstallNewPluginsAvailable(new NewWorldBuilderModel(),
            "H:\\Program Files (x86)\\Red Alert 3(Incomplete)\\CoronaLauncher_Test_3.11.9028.39760\\CoronaResources\\NewWorldBuilder\\WbLauncher.exe");
    }


}