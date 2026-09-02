namespace Core.Paths;

/// <summary>
/// AppData paths for the v2 companion. Metadata uses Ra3MapUtils.v2.db (never Ra3MapUtils.db).
/// </summary>
public static class AppDataPaths
{
    public static string UserDataPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Ra3MapUtils");

    public static string V2SqliteDbPath { get; } = Path.Combine(UserDataPath, "Ra3MapUtils.v2.db");

    public static string BackupFolderPath { get; } = Path.Combine(UserDataPath, "backup");

    public static string UserNanoProgramsPath { get; } = Path.Combine(UserDataPath, "nano_programs", "user");

    public static string OfficialNanoProgramsPath =>
        Path.Combine(AppContext.BaseDirectory, "data", "nano_programs");

    public static string LibsPath { get; } = Path.Combine(UserDataPath, "Libs");

    public static void EnsureUserDataLayout()
    {
        Directory.CreateDirectory(UserDataPath);
        Directory.CreateDirectory(UserNanoProgramsPath);
        Directory.CreateDirectory(Path.Combine(BackupFolderPath, "map"));
        Directory.CreateDirectory(LibsPath);
    }
}
