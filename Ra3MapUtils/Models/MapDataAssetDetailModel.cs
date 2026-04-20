using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ra3MapUtils.Models;

public enum MapDataAssetDetailFieldEditorType
{
    ReadOnly,
    TextBox,
    CheckBox,
    ComboBox
}

public partial class MapDataAssetDetailFieldItem : ObservableObject
{
    [ObservableProperty] private string _label = "";

    [ObservableProperty] private string _value = "-";

    [ObservableProperty] private bool _boolValue = false;

    [ObservableProperty] private bool _isGroup = false;

    [ObservableProperty] private bool _isExpanded = false;

    [ObservableProperty] private MapDataAssetDetailFieldEditorType _editorType = MapDataAssetDetailFieldEditorType.ReadOnly;

    public Action<MapDataAssetDetailFieldItem>? CommitAction { get; set; }

    public ObservableCollection<MapDataAssetDetailFieldItem> Children { get; } = new();

    public ObservableCollection<string> Options { get; } = new();
}

public class MapDataAssetDetailModel
{
    public string Title { get; set; } = "-";

    public string Description { get; set; } = "-";

    public ObservableCollection<MapDataAssetDetailFieldItem> Fields { get; } = new();

    public static MapDataAssetDetailModel Empty()
    {
        return new MapDataAssetDetailModel
        {
            Title = "-",
            Description = "-"
        };
    }
}
