using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using Ra3MapUtils.Models;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Ra3MapUtils.Services.Interface;

public interface IUpdateService
{
    public Task<bool> FillUpdateModel(UpdateModel model);
    
    public Task<bool> DownloadUpdateAsync(UpdateModel model);

    public void UpdateAndRestart(UpdateModel model);

    public void WaitExitAnUpdate(UpdateModel model);
}