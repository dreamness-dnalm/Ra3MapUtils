namespace Core.Settings;

public static class UiLanguageCodes
{
    public const string ZhCn = "zh-CN";
    public const string En = "en";

    public static bool IsSupported(string? code) =>
        string.Equals(code, ZhCn, StringComparison.OrdinalIgnoreCase)
        || string.Equals(code, En, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string code) =>
        string.Equals(code, ZhCn, StringComparison.OrdinalIgnoreCase) ? ZhCn : En;
}

public interface IUiSettingsStore
{
    /// <summary>Returns null when the user has never chosen a language.</summary>
    string? GetLanguageOrNull();

    void SetLanguage(string languageCode);
}
