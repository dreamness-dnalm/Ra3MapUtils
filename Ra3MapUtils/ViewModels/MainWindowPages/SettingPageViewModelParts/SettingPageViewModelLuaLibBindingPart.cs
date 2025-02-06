
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Microsoft.Win32;
using Ra3MapUtils.Models;
using SharedFunctionLib.Utils;
using UtilLib.utils;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject
{
    [ObservableProperty] private LuaLibBindingModel _luaLibBindingModel = new LuaLibBindingModel();

    [ObservableProperty] private string _luaLibBindingPathHint = "";

    [ObservableProperty] private Brush _luaLibBindingPathHintColor;
    
    [ObservableProperty] private Visibility _luaLibBindingUpdateNowVisibility = Visibility.Collapsed;

    [ObservableProperty] private string _luaLibBindingCurrVersion = "unknown";

    [ObservableProperty] private string _luaLibBindingUpdateHint;
    
    [ObservableProperty] private Brush _luaLibBindingUpdateHintColor;
    
    private string luaLibLastestMd5 = "";
    
    private string luaLibLatestVersion = "";

    private string luaLibLatestDownloadUrl = "";
    
    private static string luaLibUpdateBaseUrl = "http://public-files.amiksemo.com/ra3/Ra3CoronaMapLuaLib/";

    private static string luaLibUpdateVersionUrl = luaLibUpdateBaseUrl + "release.json";

    private static string downloadCachePath = Path.Combine(Ra3MapUtilsPathUtil.UserCachePath, "luaLibDownloadCache");

    public void OnLoadLuaLibBindingPart()
    {
        _luaLibBindingModel.Reload();
        OnLuaLibBindingModelChanged(_luaLibBindingModel);
        // ObservableUtil.Subscribe(_luaLibBindingModel, this);
    }
    
    [RelayCommand]
    private void PickLuaLibPath()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "选择Lua库安装路径(空目录)"
        };
        openFolderDialog.ShowDialog();
        var dir = openFolderDialog.FolderName;
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        if (! Directory.Exists(dir))
        {
            MessageBox.Show("目录不存在");
            return;
        }

        if (Directory.EnumerateFileSystemEntries(dir).Any())
        {
            MessageBox.Show("请指定一个空文件夹. 如果该文件夹已经安装了lua库, 可以先删除, 稍候会重新安装");
            return;
        }

        LuaLibBindingModel.LuaLibPath = dir;
        
        OnLuaLibBindingModelChanged(_luaLibBindingModel);
        MessageBox.Show("开始安装!");
    }

    [RelayCommand]
    private void LuaLibReinstall()
    {
        
    }
    
    [RelayCommand]
    private void LuaLibCheckUpdate()
    {
        if (!Directory.Exists(_luaLibBindingModel.LuaLibPath))
        {
            MessageBox.Show("Lua库路径不存在, 请重新指定路径");
            return;
        }
        LuaLibBindingUpdateHint = "正在检查更新...";
        LuaLibBindingUpdateHintColor = Brushes.Blue;

        var isSuccess = LuaLibFullRemoteVersionInfo();

        if (isSuccess)
        {
            OnLuaLibBindingModelChanged(_luaLibBindingModel);
            
            // todo 如果需要更新, 开始更新逻辑
        }
        else
        {
            LuaLibBindingUpdateHint = "检查更新失败, 请稍候重试";
            LuaLibBindingUpdateHintColor = Brushes.Red;
        }

        
    }
    
    partial void OnLuaLibBindingModelChanged(LuaLibBindingModel value)
    {
        if (string.IsNullOrEmpty(value.LuaLibPath))
        {
            LuaLibBindingPathHint = "未绑定";
            LuaLibBindingPathHintColor = Brushes.Red;
            LuaLibBindingCurrVersion = "unknown";
            LuaLibBindingUpdateNowVisibility = Visibility.Collapsed;
            LuaLibBindingUpdateHint = "请先绑定Lua库文件夹";
            LuaLibBindingUpdateHintColor = Brushes.Red;
        } 
        else if (! Directory.Exists(value.LuaLibPath))
        {
            LuaLibBindingPathHint = "目标文件夹不存在";
            LuaLibBindingPathHintColor = Brushes.Red;
            LuaLibBindingCurrVersion = "unknown";
            LuaLibBindingUpdateNowVisibility = Visibility.Collapsed;
            LuaLibBindingUpdateHint = "请先绑定Lua库文件夹";
            LuaLibBindingUpdateHintColor = Brushes.Red;
        }
        else
        {
            LuaLibBindingPathHint = "已绑定";
            LuaLibBindingPathHintColor = Brushes.Green;
            LuaLibBindingCurrVersion = LuaLibGetLocalVersion(value.LuaLibPath);
            LuaLibBindingUpdateHint = "最新版本: " + luaLibLatestVersion;
            if (LuaLibIsRequireUpdate(LuaLibBindingCurrVersion, luaLibLatestVersion))
            {
                LuaLibBindingUpdateNowVisibility = Visibility.Visible;
            }
            else
            {
                LuaLibBindingUpdateNowVisibility = Visibility.Collapsed;
            }
        }
        
    }

    private static bool LuaLibIsRequireUpdate(string localVersion, string remoteVersion)
    {
        if (localVersion == "unknown")
        {
            return true;
        }
        
        try
        {
            var splits1 = localVersion.Split("-");
            var splits2 = remoteVersion.Split("-");

            var versions1 = splits1[0].Split('.');
            var versions2 = splits2[0].Split('.');

            if (Convert.ToInt32(versions1[0]) < Convert.ToInt32(versions2[0]))
            {
                return true;
            }
            
            if (Convert.ToInt32(versions1[1]) < Convert.ToInt32(versions2[1]))
            {
                return true;
            }
            
            if (Convert.ToInt32(versions1[2]) < Convert.ToInt32(versions2[2]))
            {
                return true;
            }
            
            if(splits1.Length == 1 && splits2.Length == 1)
            {
                return false;
            }
            
            if(splits1.Length > splits2.Length)
            {
                return true;
            }
            if(splits1.Length < splits2.Length)
            {
                return false;
            }

            
            var suffix1 = splits1[1];
            var suffix2 = splits2[1];

            if (suffix1 == suffix2)
            {
                return false;
            }

            return true;

        }
        catch (Exception e)
        {
            Logger.WriteLog(e.Message);
            return true;
        }
    }

    private static string LuaLibGetLocalVersion(string libPath)
    {
        try
        {
            var versionFilePath = Path.Combine(libPath, "VERSION");
            if (! File.Exists(versionFilePath))
            {
                return "unknown";
            }

            return File.ReadAllText(versionFilePath);
        }catch(Exception e)
        {
            Logger.WriteLog(e.Message);
            return "unknown";
        }
    }

    private bool LuaLibFullRemoteVersionInfo()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string jsonContent = client.GetStringAsync(luaLibUpdateVersionUrl).Result;
                
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    
                    luaLibLatestVersion = root.GetProperty("Version").GetString();
                    luaLibLastestMd5 = root.GetProperty("Md5").GetString();
                    string fileName = root.GetProperty("FileName").GetString();
                    luaLibLatestDownloadUrl = luaLibUpdateBaseUrl + fileName;
                }

                return true;
            }
            catch(Exception e)
            {
                Logger.WriteLog(e.Message);
                return false;
            }
        }
    }
    
}