using System.Globalization;
using System.Windows;
using Core.Settings;

namespace UI.Services;

public sealed class LocalizationService : ILocalizationService
{
    public const string StringsDictionaryMarker = "Ra3MapUtils.Strings";

    private readonly IUiSettingsStore _settingsStore;
    private ResourceDictionary? _activeDictionary;

    public LocalizationService(IUiSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
    }

    public string CurrentLanguage { get; private set; } = UiLanguageCodes.ZhCn;

    public event EventHandler? LanguageChanged;

    public string ResolveStartupLanguage()
    {
        var saved = _settingsStore.GetLanguageOrNull();
        if (saved is not null)
        {
            return saved;
        }

        return ResolveFromOs();
    }

    public void ApplyLanguage(string languageCode, bool persist)
    {
        var normalized = UiLanguageCodes.IsSupported(languageCode)
            ? UiLanguageCodes.Normalize(languageCode)
            : ResolveFromOs();

        if (persist)
        {
            _settingsStore.SetLanguage(normalized);
        }

        SwapDictionary(normalized);
        CurrentLanguage = normalized;

        try
        {
            var culture = CultureInfo.GetCultureInfo(normalized);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch (CultureNotFoundException)
        {
            // ignore
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key, string? fallback = null)
    {
        if (_activeDictionary is not null && _activeDictionary.Contains(key))
        {
            return _activeDictionary[key] as string ?? fallback ?? key;
        }

        if (Application.Current?.TryFindResource(key) is string fromApp)
        {
            return fromApp;
        }

        return fallback ?? key;
    }

    private void SwapDictionary(string languageCode)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var uri = new Uri(
            languageCode == UiLanguageCodes.En
                ? "pack://application:,,,/Resources/Strings.en.xaml"
                : "pack://application:,,,/Resources/Strings.zh-CN.xaml",
            UriKind.Absolute);

        var dict = new ResourceDictionary { Source = uri };
        dict[StringsDictionaryMarker] = languageCode;

        var merged = app.Resources.MergedDictionaries;
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(StringsDictionaryMarker))
            {
                merged.RemoveAt(i);
            }
        }

        merged.Add(dict);
        _activeDictionary = dict;
    }

    private static string ResolveFromOs()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return UiLanguageCodes.ZhCn;
        }

        return UiLanguageCodes.En;
    }
}
