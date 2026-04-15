using System.Collections.ObjectModel;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;

namespace Ra3MapUtils.Models;

public enum MapDataAssetTreeNodeKind
{
    Root,
    Player
}

public class MapDataAssetTreeNode
{
    public string DisplayName { get; set; } = "";

    public MapDataAssetTreeNodeKind Kind { get; set; } = MapDataAssetTreeNodeKind.Root;

    public PlayerData? Player { get; set; }

    public bool IsExpanded { get; set; }

    public bool IsSelected { get; set; }

    public ObservableCollection<MapDataAssetTreeNode> Children { get; } = new();
}
