namespace Core.Debugger;

public interface IGameDebuggerService
{
    string SettingsFilePath { get; }

    string InjectorExecutablePath { get; }

    bool IsInjectorPresent { get; }

    DebuggerMapSettings LoadMapSettings();

    void SaveMapSettings(DebuggerMapSettings settings);

    /// <summary>Starts the injector process. Returns false when the binary is missing.</summary>
    bool TryLaunchInjector(out string? errorMessage);
}
