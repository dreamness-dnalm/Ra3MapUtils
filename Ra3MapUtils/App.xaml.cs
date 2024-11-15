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

        services.AddSingleton<ILuaImportService, LuaImportService>();
        services.AddSingleton<ISettingService, SettingService>();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        
        return services.BuildServiceProvider();
    }
    
    public App()
    {
        VelopackApp
            .Build()
            .WithFirstRun(i => MessageBox.Show("first run"))
            .WithRestarted(i => MessageBox.Show("restarted"))
            .WithAfterInstallFastCallback(i => MessageBox.Show("after install fast"))
            .WithAfterUpdateFastCallback(i => MessageBox.Show("after update fast"))
            .WithBeforeUninstallFastCallback(i => MessageBox.Show("before uninstall fast"))
            .WithBeforeUpdateFastCallback(i => MessageBox.Show("before update fast"))
            .Run();
        Services = ConfigureServices();
        InitializeComponent();

        Directory.CreateDirectory(Ra3MapUtilsPathUtil.UserDataPath);
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }
}