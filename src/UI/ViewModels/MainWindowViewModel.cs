using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UI.Services;
using UI.Views.Pages;

namespace UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILocalizationService _localization;

    public MainWindowViewModel(
        MapManagePage mapManagePage,
        NanoProgramsPage nanoProgramsPage,
        ToolBoxPage toolBoxPage,
        SettingPage settingPage,
        ILocalizationService localization)
    {
        _localization = localization;

        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new("maps", localization.GetString("Nav_Maps", "地图"), "\uE707", mapManagePage, "Nav_Maps"),
            new("nano", localization.GetString("Nav_Nano", "微程序"), "\uE943", nanoProgramsPage, "Nav_Nano"),
            new("toolbox", localization.GetString("Nav_Toolbox", "工具箱"), "\uE74C", toolBoxPage, "Nav_Toolbox"),
        };

        SettingsNavigationItem = new NavigationItem(
            "settings",
            localization.GetString("Nav_Settings", "设置"),
            "\uE713",
            settingPage,
            "Nav_Settings");

        _selectedNavigationItem = NavigationItems[0];
        RefreshChrome();
        _localization.LanguageChanged += (_, _) => RefreshLocalizedChrome();
    }

    [ObservableProperty]
    private string _title = "RA3地编伴侣";

    [ObservableProperty]
    private string _subtitle = "地图编辑辅助工具";

    [ObservableProperty]
    private string _settingsNavTitle = "设置";

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public NavigationItem SettingsNavigationItem { get; }

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    /// <summary>Feature-list selection only (excludes bottom-pinned Settings).</summary>
    public NavigationItem? SelectedFeatureItem
    {
        get => SelectedNavigationItem is not null
               && SelectedNavigationItem.Key != "settings"
            ? SelectedNavigationItem
            : null;
        set
        {
            if (value is not null)
            {
                SelectedNavigationItem = value;
            }
        }
    }

    public bool IsSettingsSelected => SelectedNavigationItem?.Key == "settings";

    public object? CurrentContent => SelectedNavigationItem?.Content;

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        OnPropertyChanged(nameof(CurrentContent));
        OnPropertyChanged(nameof(SelectedFeatureItem));
        OnPropertyChanged(nameof(IsSettingsSelected));
    }

    [RelayCommand]
    private void OpenSettings()
    {
        SelectedNavigationItem = SettingsNavigationItem;
    }

    public void NavigateTo(string key)
    {
        switch (key)
        {
            case "maps":
            case "nano":
            case "toolbox":
                var feature = NavigationItems.FirstOrDefault(i =>
                    string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));
                if (feature is not null)
                {
                    SelectedNavigationItem = feature;
                }

                break;
            case "settings":
                SelectedNavigationItem = SettingsNavigationItem;
                break;
        }
    }

    private void RefreshChrome()
    {
        Title = _localization.GetString("App_Title", "RA3地编伴侣");
        Subtitle = _localization.GetString("App_Subtitle", "地图编辑辅助工具");
        SettingsNavTitle = _localization.GetString("Nav_Settings", "设置");
    }

    private void RefreshLocalizedChrome()
    {
        RefreshChrome();
        foreach (var item in NavigationItems)
        {
            if (item.TitleKey is not null)
            {
                item.Title = _localization.GetString(item.TitleKey, item.Title);
            }
        }

        if (SettingsNavigationItem.TitleKey is not null)
        {
            SettingsNavigationItem.Title =
                _localization.GetString(SettingsNavigationItem.TitleKey, SettingsNavigationItem.Title);
        }

        SettingsNavTitle = SettingsNavigationItem.Title;
    }
}
