using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Ra3MapUtils.Views;

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
        
        services.AddSingleton<LuaManagerWindow>();
        services.AddSingleton<LuaManagerWindowViewModel>();

        return services.BuildServiceProvider();
    }
    
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }
}