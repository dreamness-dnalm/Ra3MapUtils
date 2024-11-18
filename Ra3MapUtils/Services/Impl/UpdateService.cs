using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Utils;
using UtilLib.utils;
using Velopack;
using Velopack.Sources;

namespace Ra3MapUtils.Services.Impl;

public class UpdateService: IUpdateService
{
    private UpdateManager mgr = new UpdateManager(
        // new GiteeSource("dreamness", "update_test", null, true),
        new SimpleWebSource("http://public-files.amiksemo.com/ra3/Ra3MapUtils"),
        null, 
        null);
    
    public async Task<bool> FillUpdateModel(UpdateModel model)
    {
        if (model.IsCheckingUpdate || model.IsDownloadingUpdate || model.IsDownloadUpdateFinished || model.IsAreadyUpdated)
        {
            return true;
        }
        
        try
        {
            model.IsCheckingUpdate = true;
            var newVersion = await mgr.CheckForUpdatesAsync();
            model.IsCheckingUpdate = false;
            model.UpdateInfo = newVersion;
            if (newVersion == null)
            {
                model.IsUpdateAvailable = false;
            }
            else
            {
                model.LatestVersionStr = "v" + newVersion.TargetFullRelease.Version.ToNormalizedString();
                model.IsUpdateAvailable = true;
                model.ReleaseNotesHtml = newVersion.TargetFullRelease.NotesHTML;
                Logger.WriteLog(model.ReleaseNotesHtml);
            }

            model.IsCheckingUpdateError = false;
            return true;
        }
        catch (Exception e)
        {
            Logger.WriteLog("update error: " + e.Message);
            Logger.WriteLog(e.StackTrace);
            model.IsCheckingUpdate = false;
            model.IsCheckingUpdateError = true;
            model.IsUpdateAvailable = false;
            model.UpdateInfo = null;
            model.LatestVersionStr = "";
            model.IsCheckingUpdateError = true;
            return false;
        }
    }


    public async Task<bool> DownloadUpdateAsync(UpdateModel model)
    {
        if (model.IsCheckingUpdate || model.IsDownloadingUpdate || model.IsDownloadUpdateFinished || model.IsAreadyUpdated)
        {
            return true;
        }
        
        if (model.IsUpdateAvailable)
        {
            model.IsDownloadUpdateError = false;
            model.IsDownloadUpdateFinished = false;
            try
            {
                var downloadUpdatesAsync = mgr.DownloadUpdatesAsync(model.UpdateInfo, i => model.DownloadProgress = i);
                model.IsDownloadingUpdate = true;
                await downloadUpdatesAsync;
                model.IsDownloadingUpdate = false;
                model.IsDownloadUpdateFinished = true;
                model.IsCheckingUpdateError = false;
            }
            catch (Exception e)
            {
                model.IsDownloadUpdateError = true;
                model.IsDownloadUpdateFinished = false;
                model.IsDownloadingUpdate = false;
            }
        }

        return true;
    }

    public void UpdateAndRestart(UpdateModel model)
    {
        if (model.IsDownloadUpdateFinished)
        {
            mgr.ApplyUpdatesAndRestart(model.UpdateInfo);
            model.IsAreadyUpdated = true;
        }
    }

    public void WaitExitAnUpdate(UpdateModel model)
    {
        if (model.IsDownloadUpdateFinished)
        {
            mgr.WaitExitThenApplyUpdatesAsync(model.UpdateInfo);
            model.IsAreadyUpdated = true;
        }
    }
}