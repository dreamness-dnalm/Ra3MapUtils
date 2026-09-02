using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Toolbox;
using UI.Services;

namespace UI.ViewModels;

public partial class ToolBoxPageViewModel : ObservableObject
{
    private readonly IToolboxCatalog _catalog;
    private readonly IToolboxLauncher _launcher;
    private readonly ILocalizationService _localization;

    public ObservableCollection<ToolboxCategoryGroupViewModel> Categories { get; } = new();

    [ObservableProperty] private string _statusText = "就绪";

    public ToolBoxPageViewModel(
        IToolboxCatalog catalog,
        IToolboxLauncher launcher,
        ILocalizationService localization)
    {
        _catalog = catalog;
        _launcher = launcher;
        _localization = localization;
        RebuildCategories();
        StatusText = _localization.GetString("Toolbox_Ready", "就绪");
        _localization.LanguageChanged += (_, _) =>
        {
            RebuildCategories();
            StatusText = _localization.GetString("Toolbox_Ready", "就绪");
        };
    }

    [RelayCommand]
    private void Launch(ToolboxToolItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        var result = _launcher.Launch(item.Entry);
        StatusText = result.Success
            ? string.Format(_localization.GetString("Toolbox_OpenedFormat"), item.Title)
            : (result.Message ?? string.Format(_localization.GetString("Toolbox_OpenFailedFormat"), item.Title));
    }

    private void RebuildCategories()
    {
        Categories.Clear();
        foreach (var group in _catalog.GetTools()
                     .GroupBy(t => t.Category)
                     .OrderBy(g => g.Key))
        {
            var categoryKey = group.Key switch
            {
                ToolboxCategory.MapAndData => "Category_MapAndData",
                ToolboxCategory.Developer => "Category_Developer",
                _ => "Category_Other",
            };
            Categories.Add(new ToolboxCategoryGroupViewModel(
                _localization.GetString(categoryKey, ToolboxCategoryLabels.GetDisplayName(group.Key)),
                group.Select(t => new ToolboxToolItemViewModel(t, _localization)).ToList()));
        }
    }
}

public sealed class ToolboxCategoryGroupViewModel
{
    public ToolboxCategoryGroupViewModel(string title, IReadOnlyList<ToolboxToolItemViewModel> tools)
    {
        Title = title;
        Tools = tools;
    }

    public string Title { get; }

    public IReadOnlyList<ToolboxToolItemViewModel> Tools { get; }
}

public sealed class ToolboxToolItemViewModel
{
    private readonly ILocalizationService _localization;

    public ToolboxToolItemViewModel(ToolEntry entry, ILocalizationService localization)
    {
        Entry = entry;
        _localization = localization;
        var availability = entry.GetAvailability();
        IsAvailable = availability.IsAvailable;
        UnavailableReason = availability.UnavailableReason;
    }

    public ToolEntry Entry { get; }

    public string Id => Entry.Id;

    public string Title => _localization.GetString($"Tool_{Entry.Id}_Title", Entry.Title);

    public string Description => _localization.GetString($"Tool_{Entry.Id}_Description", Entry.Description);

    public string Glyph => Entry.Glyph;

    public bool IsAvailable { get; }

    public string? UnavailableReason { get; }

    public double Opacity => IsAvailable ? 1.0 : 0.45;
}
