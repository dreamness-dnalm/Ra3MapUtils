using Core.Settings;

namespace Core.NanoPrograms;

/// <summary>
/// Resolves bilingual nano-program name/description for zh-CN / en with cross-fallback.
/// </summary>
public static class NanoProgramDisplayResolver
{
    public static string ResolveName(NanoProgramInfoModel info, string languageCode)
    {
        var preferEn = IsEnglish(languageCode);
        return Pick(preferEn ? info.NameEn : info.Name, preferEn ? info.Name : info.NameEn, info.ID);
    }

    public static string ResolveDescription(NanoProgramInfoModel info, string languageCode)
    {
        var preferEn = IsEnglish(languageCode);
        return Pick(preferEn ? info.DescriptionEn : info.Description, preferEn ? info.Description : info.DescriptionEn, string.Empty);
    }

    public static bool MatchesSearch(NanoProgramInfoModel info, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        var k = keyword.Trim();
        return Contains(info.Name, k)
               || Contains(info.NameEn, k)
               || Contains(info.Description, k)
               || Contains(info.DescriptionEn, k)
               || Contains(info.ID, k);
    }

    /// <summary>
    /// Returns a shallow copy whose <see cref="NanoProgramInfoModel.Name"/> /
    /// <see cref="NanoProgramInfoModel.Description"/> are resolved for <paramref name="languageCode"/>.
    /// </summary>
    public static NanoProgramModel WithResolvedDisplay(NanoProgramModel source, string languageCode)
    {
        var info = source.Info;
        return new NanoProgramModel
        {
            Info = new NanoProgramInfoModel
            {
                ID = info.ID,
                Name = ResolveName(info, languageCode),
                Description = ResolveDescription(info, languageCode),
                NameEn = info.NameEn,
                DescriptionEn = info.DescriptionEn,
                Path = info.Path,
                InstallType = info.InstallType,
            },
            IsEnabled = source.IsEnabled,
            IsWbVisible = source.IsWbVisible,
            Order = source.Order,
        };
    }

    private static bool IsEnglish(string languageCode) =>
        string.Equals(UiLanguageCodes.Normalize(languageCode), UiLanguageCodes.En, StringComparison.OrdinalIgnoreCase);

    private static string Pick(string preferred, string fallback, string lastResort)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred.Trim();
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback.Trim();
        }

        return lastResort ?? string.Empty;
    }

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrEmpty(haystack)
        && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
