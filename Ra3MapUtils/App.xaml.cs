using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.AspNetCore.Builder;
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
using Newtonsoft.Json;
using Ra3MapUtils.MCP;
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

    private WebApplication? _webApp;

    private static IServiceProvider ConfigureServices()
    {
        
        var services = new ServiceCollection();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<HomePage>();
        services.AddSingleton<HomePageViewModel>();
        
        services.AddSingleton<ToolBoxPage>();
        services.AddSingleton<ToolBoxPageViewModel>();

        services.AddSingleton<KnowledgeBasePage>();
        services.AddSingleton<KnowledgeBasePageViewModel>();
        
        services.AddSingleton<AboutPage>();
        services.AddSingleton<AboutPageViewModel>();

        services.AddSingleton<MapManagePage>();
        services.AddSingleton<MapManagePageViewModel>();
        
        services.AddSingleton<SettingPage>();
        services.AddSingleton<SettingPageViewModel>();
        
        services.AddSingleton<ScriptListPage>();
        services.AddSingleton<ScriptListPageViewModel>();
        
        services.AddSingleton<AIPage>();
        services.AddSingleton<AIPageViewModel>();

        services.AddTransient<LuaManagerWindow>();
        services.AddTransient<LuaManagerWindowViewModel>();

        services.AddTransient<LuaImportItemControl>();
        services.AddTransient<LuaManagerWindowViewModel>();

        services.AddTransient<BorderManagerWindow>();
        services.AddTransient<BorderManagerWindowViewModel>();

        services.AddTransient<CodeEditorWindow>();
        services.AddTransient<CodeEditorWindowViewModel>();

        services.AddTransient<LogViewerWindow>();
        services.AddSingleton<LogViewerWindowViewModel>();

        services.AddTransient<ChatLuaHelperViewWindow>();
        services.AddSingleton<ChatLuaHelperViewModel>();
        
        services.AddTransient<TerrainTransWindow>();
        services.AddSingleton<TerrainTransWindowViewModel>();

        services.AddSingleton<ILuaImportService, LuaImportService>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<INewWorldBuilderPluginService, NewWorldBuilderPluginService>();
        services.AddSingleton<IMapDataOperateService, MapDataOperateService>();
        services.AddSingleton<INanoProgramService, NanoProgramService>();
        
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
        settingPageViewModel.OnLoadLuaLibBindingPart();
    }

    // public static APIService _apiService = new();

    private static Mutex _mutex;
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // _apiService.Start();
        
        _mutex = new Mutex(true, "Dreamness.RA3.Ra3MapUtils", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("地编伴侣已经请启动. \n右键点击系统托盘图标可显示菜单.\n如果没有, 可通过任务管理器结束进程后重新启动.", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            Environment.Exit(0);
        }


        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(AppContext.BaseDirectory);
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(Path.Combine(Ra3MapUtilsPathUtil.UserDataPath, "Libs"));

        // builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();
        
        _webApp = builder.Build();
        
        _webApp.Urls.Add("http://127.0.0.1:30033");
        
        _webApp.UseSwagger();
        _webApp.UseSwaggerUI();
        
        _webApp.MapControllers();
        _webApp.MapMcp("/mcp");
        
        await _webApp.StartAsync();

    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        // _apiService.Stop();
        // base.OnExit(e);
        if (_webApp is not null)
        {
            await _webApp.StopAsync();
            _webApp.DisposeAsync();
        }

        if (_mutex is not null)
        {
            _mutex.ReleaseMutex();
        }
        
        base.OnExit(e);
    }
}