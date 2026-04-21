using System.Collections.ObjectModel;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Script;

namespace Ra3MapUtils.Models;

public enum MapDataAssetTreeNodeKind
{
    PlayerRoot,
    Player,
    ScriptRoot,
    ScriptList,
    ScriptGroup,
    Script,
    ScriptIfRoot,
    ScriptThenRoot,
    ScriptElseRoot,
    ScriptOrCondition,
    ScriptCondition,
    ScriptActionTrue,
    ScriptActionFalse
}

public class MapDataAssetTreeNode
{
    public string DisplayName { get; set; } = "";

    public MapDataAssetTreeNodeKind Kind { get; set; } = MapDataAssetTreeNodeKind.PlayerRoot;

    public PlayerData? Player { get; set; }

    public ScriptList? ScriptList { get; set; }

    public ScriptGroup? ScriptGroup { get; set; }

    public Script? Script { get; set; }

    public OrCondition? OrCondition { get; set; }

    public ScriptConditionContent? ScriptCondition { get; set; }

    public ScriptAction? ScriptAction { get; set; }

    public int? ScriptListIndex { get; set; }

    public int? OrConditionIndex { get; set; }

    public bool IsExpanded { get; set; }

    public bool IsSelected { get; set; }

    public ObservableCollection<MapDataAssetTreeNode> Children { get; } = new();

    public bool CanAddScript => Kind is MapDataAssetTreeNodeKind.ScriptList or MapDataAssetTreeNodeKind.ScriptGroup;

    public bool CanDeleteScript => Kind == MapDataAssetTreeNodeKind.Script;

    public bool CanRenameScript => Kind == MapDataAssetTreeNodeKind.Script;

    public bool CanAddScriptGroup => Kind is MapDataAssetTreeNodeKind.ScriptList or MapDataAssetTreeNodeKind.ScriptGroup;

    public bool CanDeleteScriptGroup => Kind == MapDataAssetTreeNodeKind.ScriptGroup;

    public bool CanRenameScriptGroup => Kind == MapDataAssetTreeNodeKind.ScriptGroup;
}
