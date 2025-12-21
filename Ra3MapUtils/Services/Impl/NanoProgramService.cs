using System.IO;
using Dreamness.ScriptExecutor;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Business;
using SharedFunctionLib.Models;

namespace Ra3MapUtils.Services.Impl;

public class NanoProgramService: INanoProgramService
{
    private Dictionary<NanoProgramInstallType, string> _nanoProgramDir = new Dictionary<NanoProgramInstallType, string>
    {
        [NanoProgramInstallType.Official] = Path.Combine(AppContext.BaseDirectory, "data", "nano_programs"),
        [NanoProgramInstallType.User] = Path.Combine(SharedFunctionLib.Utils.Ra3MapUtilsPathUtil.UserDataPath, "nano_programs", "user"),
        [NanoProgramInstallType.Store] = Path.Combine(SharedFunctionLib.Utils.Ra3MapUtilsPathUtil.UserDataPath, "nano_programs", "store")
    };
    
    
    public List<NanoProgramModel> GetNanoPrograms()
    {
        var simpleNanoProgramMetaModels = NanoProgramMetaBusiness.GetAll();
        var usedIds = new HashSet<string>();
        
        var resList = new List<NanoProgramModel>();

        foreach (var p in _nanoProgramDir)
        {
            if (Directory.Exists(p.Value))
            {
                var programDirs = Directory.GetDirectories(p.Value);
                foreach (var dir in programDirs)
                {
                    var info = NanoProgramInfoModel.Of(dir, p.Key);
                    if (info == null)
                    {
                        continue;
                    }

                    var record = simpleNanoProgramMetaModels.Where(i => i.ID == info.ID)
                        .FirstOrDefault((SimpleNanoProgramMetaModel)null);

                    int order = -1;
                    bool isEnabled = true;
                    bool isWbVisible = true;
                    if (record != null)
                    {
                        order = record.Order;
                        isEnabled = record.IsEnabled == 1;
                        isWbVisible = record.IsWbVisible == 1;
                    }

                    NanoProgramMetaBusiness.AddOrUpdate(info.ID, isEnabled, isWbVisible, order);
                    usedIds.Add(info.ID);
                    
                    resList.Add(new NanoProgramModel
                    {
                        Info = info,
                        IsEnabled = isEnabled,
                        IsWbVisible = isWbVisible,
                        Order = order
                    });
                }
            }
        }
        NanoProgramMetaBusiness.DeleteUnused(usedIds.ToList());
        
        return resList.OrderBy(x => x.Order).ToList();
    }

    public ScriptExecutionResult ExecuteNanoProgram(string id, Dictionary<string, string> arguments)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var allProgramModel = GetNanoPrograms();
        var record = allProgramModel
            .Where(i => i.Info.ID == id)
            .FirstOrDefault((NanoProgramModel)null);
        if (record == null)
        {
            throw new Exception($"Nano program not found: {id}");
        }

        if (!record.IsEnabled)
        {
            throw new Exception($"Nano program is disabled: {id}");
        }

        var csFilePath = Path.Combine(record.Info.Path, "Main.cs");
        if(!File.Exists(csFilePath))
        {
            throw new FileNotFoundException($"Nano program Main.cs not found: {csFilePath}");
        }
        
        var code = System.IO.File.ReadAllText(csFilePath);

        var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
        scriptOptions = ScriptExecutor.WithLoadedAssemblies(
            scriptOptions,
            excludeSystemAssemblies: true,
            excludeDynamicAssemblies: true
        );
        var executor = new ScriptExecutor(scriptOptions);

        return executor.Execute(code, workingDirectory: record.Info.Path, globals:CSharpProgramGlobals.Of(arguments), globalsType: typeof(CSharpProgramGlobals));
        
    }
}
