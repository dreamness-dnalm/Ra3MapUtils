using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels;

public partial class NavigationItem : ObservableObject
{
    public NavigationItem(string key, string title, string glyph, object content, string? titleKey = null)
    {
        Key = key;
        Title = title;
        Glyph = glyph;
        Content = content;
        TitleKey = titleKey;
    }

    public string Key { get; }

    public string? TitleKey { get; }

    [ObservableProperty]
    private string _title;

    /// <summary>Segoe Fluent Icons glyph.</summary>
    public string Glyph { get; }

    public object Content { get; }
}
