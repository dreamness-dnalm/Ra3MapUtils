using System.Globalization;
using Core.Settings;

namespace UI.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }

    event EventHandler? LanguageChanged;

    /// <summary>Resolve effective language: saved preference, else OS default. Does not persist.</summary>
    string ResolveStartupLanguage();

    void ApplyLanguage(string languageCode, bool persist);

    string GetString(string key, string? fallback = null);
}
