using System.Windows;
using Core.Debugger;
using Core.Toolbox;
using Microsoft.Extensions.DependencyInjection;
using UI.Views.Tools;

namespace UI.Services;

public sealed class ToolboxLauncher : IToolboxLauncher
{
    private readonly IServiceProvider _services;
    private readonly ILocalizationService _localization;
    private readonly Dictionary<string, Window> _openWindows = new(StringComparer.OrdinalIgnoreCase);

    public ToolboxLauncher(IServiceProvider services, ILocalizationService localization)
    {
        _services = services;
        _localization = localization;
    }

    public ToolboxLaunchResult Launch(ToolEntry entry)
    {
        var availability = entry.GetAvailability();
        if (!availability.IsAvailable)
        {
            var reason = string.IsNullOrWhiteSpace(availability.UnavailableReason)
                ? _localization.GetString("Toolbox_Unavailable")
                : availability.UnavailableReason!;
            MessageBox.Show(reason, entry.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            return ToolboxLaunchResult.Fail(reason);
        }

        if (entry.Id == StaticToolboxCatalog.OpenDebuggerId)
        {
            var debugger = _services.GetRequiredService<IGameDebuggerService>();
            if (!debugger.TryLaunchInjector(out var errorMessage))
            {
                var message = errorMessage
                    ?? _localization.GetString("DbgLaunch_Failed", "启动调试工具失败。");
                MessageBox.Show(message, entry.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return ToolboxLaunchResult.Fail(message);
            }

            return ToolboxLaunchResult.Ok();
        }

        if (_openWindows.TryGetValue(entry.Id, out var existing) && existing.IsLoaded)
        {
            existing.Activate();
            return ToolboxLaunchResult.Ok();
        }

        Window window = entry.Id switch
        {
            StaticToolboxCatalog.FastHashId =>
                _services.GetRequiredService<FastHashCalculatorWindow>(),
            StaticToolboxCatalog.ImageEncodingId =>
                _services.GetRequiredService<ImageEncodingToolWindow>(),
            StaticToolboxCatalog.DeveloperHostingId =>
                _services.GetRequiredService<DeveloperHostingWindow>(),
            StaticToolboxCatalog.DebuggerMapPathId =>
                _services.GetRequiredService<DebuggerMapSettingsWindow>(),
            StaticToolboxCatalog.TimeControlId =>
                _services.GetRequiredService<TimeControlWindow>(),
            StaticToolboxCatalog.LuaExecutorId =>
                _services.GetRequiredService<LuaExecutorWindow>(),
            _ => throw new InvalidOperationException($"未注册的工具启动器：{entry.Id}"),
        };

        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => _openWindows.Remove(entry.Id);
        _openWindows[entry.Id] = window;
        window.Show();
        return ToolboxLaunchResult.Ok();
    }
}
