using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
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
    private static readonly Uri StartupTelemetryUri = new("https://ra3maputils-server.amiksemo.com/file/ra3/Ra3MapUtils/releases.win.json");

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
        
        services.AddSingleton<NanoProgramPage>();
        services.AddSingleton<NanoProgramPageViewModel>();

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

        services.AddTransient<MapDataEditorWindow>();
        services.AddTransient<MapDataEditorWindowViewModel>();

        services.AddTransient<ImageEncodingToolWindow>();
        services.AddTransient<ImageEncodingToolWindowViewModel>();

        services.AddTransient<FastHashCalculatorWindow>();
        services.AddTransient<FastHashCalculatorWindowViewModel>();

        services.AddTransient<LuaExecutorWindow>();
        services.AddTransient<LuaExecutorWindowViewModel>();
        services.AddSingleton<ILuaCompletionService, LuaCompletionService>();

        services.AddTransient<TimeControlWindow>();
        services.AddTransient<TimeControlWindowViewModel>();
        
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

    private static void TrackStartupTelemetry()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using var response = await httpClient.GetAsync(
                    StartupTelemetryUri,
                    HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            }
            catch
            {
                // Startup telemetry must never block or interrupt the app.
            }
        });
    }
    
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

        TrackStartupTelemetry();

        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(AppContext.BaseDirectory);
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(Path.Combine(Ra3MapUtilsPathUtil.UserDataPath, "Libs"));

        // builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RA3地编伴侣 HTTP API 接口文档",
                Version = "v1",
                Description = "RA3地编伴侣本地 HTTP API 文档，包含状态检测、Lua4 语法检查、C# 脚本执行与微程序调用等接口。"
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });
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
