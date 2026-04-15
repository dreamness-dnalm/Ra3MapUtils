using System.Collections.ObjectModel;

namespace Ra3MapUtils.Models;

public class MapDataAssetDetailFieldItem
{
    public string Label { get; set; } = "";

    public string Value { get; set; } = "-";
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
