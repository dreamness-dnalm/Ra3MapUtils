using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Services.Controls;
using Ra3MapUtils.Services.Impl;
using Ra3MapUtils.Services.Interface;
using Ra3MapUtils.ViewModels;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.Views;
using Ra3MapUtils.Views.MainWindowPages;
using Ra3MapUtils.Views.SubWindows;
using SharedFunctionLib.Utils;
using Velopack;
using Wpf.Ui;
using Microsoft.Extensions.Logging;
using Ra3MapUtils.ViewModels.toolbox;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SettingPageViewModel = Ra3MapUtils.ViewModels.MainWindowPages.SettingPageViewModel;

namespace Ra3MapUtils;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; }

    public new static App Current => (App)Application.Current;

    private static IServiceProvider ConfigureServices()
    {
        
        var services = new ServiceCollection();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<HomePage>();
        services.AddSingleton<HomePageViewModel>();
        
        services.AddSingleton<ToolBoxPage>();
        services.AddSingleton<ToolBoxPageViewModel>();
        
        services.AddSingleton<AboutPage>();
        services.AddSingleton<AboutPageViewModel>();

        services.AddSingleton<MapManagePage>();
        services.AddSingleton<MapManagePageViewModel>();
        
        services.AddSingleton<SettingPage>();
        services.AddSingleton<SettingPageViewModel>();

        services.AddTransient<LuaManagerWindow>();
        services.AddTransient<LuaManagerWindowViewModel>();

        services.AddTransient<LuaImportItemControl>();
        services.AddTransient<LuaManagerWindowViewModel>();

        services.AddTransient<CodeEditorWindow>();
        services.AddTransient<CodeEditorWindowViewModel>();

        services.AddSingleton<LogViewerWindow>();
        services.AddSingleton<LogViewerWindowViewModel>();

        services.AddSingleton<ILuaImportService, LuaImportService>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<INewWorldBuilderPluginService, NewWorldBuilderPluginService>();
        
        
        
        return services.BuildServiceProvider();
    }
    
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();

        Directory.CreateDirectory(Ra3MapUtilsPathUtil.UserDataPath);
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        VelopackApp
            .Build()
            // .WithFirstRun(i => MessageBox.Show("first run2"))
            // .WithRestarted(i => MessageBox.Show("restarted2"))
            // .WithAfterInstallFastCallback(i => MessageBox.Show("after install fast2"))
            // .WithAfterUpdateFastCallback(i => MessageBox.Show("after update fast2"))
            // .WithBeforeUninstallFastCallback(i => MessageBox.Show("before uninstall fast2"))
            // .WithBeforeUpdateFastCallback(i => MessageBox.Show("before update fast2"))
            .Run();

        var settingPageViewModel = Services.GetRequiredService<SettingPageViewModel>();
        settingPageViewModel.OnLoadUpdatePart();
        settingPageViewModel.UpdateNow();
        settingPageViewModel.OnLoadNewWorldBuilderPart();
    }
}