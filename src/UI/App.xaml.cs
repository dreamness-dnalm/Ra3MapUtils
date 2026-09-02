using System.IO;
using System.Windows;
using Core.Debugger;
using Core.LuaImport;
using Core.Maps;
using Core.NanoPrograms;
using Core.Paths;
using Core.Settings;
using Core.Toolbox;
using Core.Updates;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using UI.MCP;
using UI.Services;
using UI.Services.ImageEncoding;
using UI.Services.Updates;
using UI.ViewModels;
using UI.ViewModels.Tools;
using UI.Views.Pages;
using UI.Views.Tools;

namespace UI;

public partial class App : Application
{
    private Mutex? _mutex;
    private WebApplication? _webApp;
    private ServiceProvider? _services;
    private SystemTrayService? _tray;
    private EventWaitHandle? _activateEvent;
    private CancellationTokenSource? _activationCts;
    private MainWindow? _mainWindow;

    public IServiceProvider Services =>
        _services ?? throw new InvalidOperationException("Application services are not initialized.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mutex = new Mutex(true, "Ra3MapUtils", out var createdNew);

        var services = new ServiceCollection();
        ConfigureAppServices(services);
        _services = services.BuildServiceProvider();

        var localization = _services.GetRequiredService<ILocalizationService>();
        localization.ApplyLanguage(localization.ResolveStartupLanguage(), persist: false);

        if (!createdNew)
        {
            SingleInstanceActivation.TrySignal();
            MessageBox.Show(
                localization.GetString("Dialog_AlreadyRunning"),
                localization.GetString("Dialog_Tip", "提示"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        AppDataPaths.EnsureUserDataLayout();
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(AppContext.BaseDirectory);
        CSharpScriptService.AssemblyAutoLoader.LoadAllAssembliesFromDirectory(AppDataPaths.LibsPath);

        try
        {
            await StartHttpHostAsync(_services);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format(localization.GetString("Dialog_HttpStartFailedBody"), ex.Message),
                localization.GetString("Dialog_HttpStartFailedTitle", "启动失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        _mainWindow = _services.GetRequiredService<MainWindow>();
        var viewModel = _services.GetRequiredService<MainWindowViewModel>();
        _tray = new SystemTrayService(_mainWindow, viewModel, localization);
        _tray.Start();

        StartActivationWatcher();

        _mainWindow.Show();

        _ = ConfirmUpdateHealthAsync();
    }

    private async Task ConfirmUpdateHealthAsync()
    {
        try
        {
            var updates = _services?.GetService<IAppUpdateService>();
            if (updates is null)
            {
                return;
            }

            await updates.ConfirmHealthyStartIfNeededAsync();
        }
        catch
        {
            // Health confirm failures are handled by Bootstrapper timeout/rollback.
        }
    }

    private void StartActivationWatcher()
    {
        _activateEvent = SingleInstanceActivation.CreateWaiter();
        _activationCts = new CancellationTokenSource();
        var token = _activationCts.Token;
        var waitHandle = _activateEvent;

        _ = Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (waitHandle.WaitOne(500))
                    {
                        Dispatcher.Invoke(() => _mainWindow?.ShowFromTray());
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, token);
    }

    private static void ConfigureAppServices(IServiceCollection services)
    {
        services.AddSingleton<IUiSettingsStore, SqliteUiSettingsStore>();
        services.AddSingleton<ILuaImportSettingsStore, SqliteLuaImportSettingsStore>();
        services.AddSingleton<ILuaLibConfigStore, SqliteLuaLibConfigStore>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        services.AddSingleton<IMapCatalogService, MapCatalogService>();
        services.AddSingleton<IMapFileService, Core.Maps.MapFileService>();
        services.AddSingleton<INanoProgramMetaStore, SqliteNanoProgramMetaStore>();
        services.AddSingleton<NanoProgramCatalog>();
        services.AddSingleton<INanoProgramService, NanoProgramService>();
        services.AddSingleton<ILuaImportService, LuaImportService>();

        services.AddSingleton<IGameDebuggerService, GameDebuggerService>();
        services.AddSingleton<IAppUpdateService, AssetCenterAppUpdateService>();
        services.AddSingleton<IToolboxCatalog, StaticToolboxCatalog>();
        services.AddSingleton<IToolboxLauncher, ToolboxLauncher>();
        services.AddSingleton<IImageEncodingService, ImageEncodingService>();

        services.AddTransient<MapManagePageViewModel>();
        services.AddTransient<MapManagePage>();
        services.AddTransient<NanoProgramsPageViewModel>();
        services.AddTransient<NanoProgramsPage>();
        services.AddTransient<ToolBoxPageViewModel>();
        services.AddTransient<ToolBoxPage>();
        services.AddTransient<SettingPageViewModel>();
        services.AddTransient<SettingPage>();
        services.AddTransient<LuaImportManagerWindowViewModel>();
        services.AddTransient<LuaImportManagerWindow>();
        services.AddTransient<FastHashCalculatorWindowViewModel>();
        services.AddTransient<FastHashCalculatorWindow>();
        services.AddTransient<ImageEncodingToolWindowViewModel>();
        services.AddTransient<ImageEncodingToolWindow>();
        services.AddTransient<DeveloperHostingWindowViewModel>();
        services.AddTransient<DeveloperHostingWindow>();
        services.AddTransient<DebuggerMapSettingsWindowViewModel>();
        services.AddTransient<DebuggerMapSettingsWindow>();
        services.AddTransient<TimeControlWindowViewModel>();
        services.AddTransient<TimeControlWindow>();
        services.AddTransient<LuaExecutorWindowViewModel>();
        services.AddTransient<LuaExecutorWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }

    private async Task StartHttpHostAsync(IServiceProvider appServices)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(App).Assembly);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RA3地编伴侣 HTTP API 接口文档",
                Version = "v1",
                Description = "RA3地编伴侣本地 HTTP API 文档，包含状态检测、图片编码、Lua4 语法检查、C# 脚本执行与微程序调用等接口。",
            });
        });
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        builder.Services.AddSingleton(appServices.GetRequiredService<INanoProgramService>());
        builder.Services.AddSingleton(appServices.GetRequiredService<IImageEncodingService>());
        builder.Services.AddSingleton(appServices.GetRequiredService<ILuaImportService>());

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
        _activationCts?.Cancel();
        _activationCts?.Dispose();
        _activationCts = null;

        _activateEvent?.Dispose();
        _activateEvent = null;

        _tray?.Dispose();
        _tray = null;

        if (_mainWindow is not null)
        {
            _mainWindow.AllowClose();
        }

        if (_webApp is not null)
        {
            await _webApp.StopAsync();
            await _webApp.DisposeAsync();
        }

        _services?.Dispose();

        if (_mutex is not null)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch
            {
                // ignore
            }

            _mutex.Dispose();
        }

        base.OnExit(e);
    }
}
