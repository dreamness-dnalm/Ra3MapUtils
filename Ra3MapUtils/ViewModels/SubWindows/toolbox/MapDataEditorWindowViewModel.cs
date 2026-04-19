using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dreamness.RA3.Map.Parser.Asset.ScriptData;
using Dreamness.RA3.Map.Parser.Core.MapScb;
using Dreamness.Ra3.Map.Parser.Asset.Base;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Script;
using Dreamness.Ra3.Map.Parser.Core.Base;
using Dreamness.Ra3.Map.Parser.Core.Map;
using Ra3MapUtils.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class MapDataEditorWindowViewModel : ObservableObject
{
    private const string PlayerRootDisplayName = "\u73a9\u5bb6\u5217\u8868";
    private const string ScriptRootDisplayName = "\u811a\u672c";
    private const string NeutralPlayerDisplayName = "(neutral)";

    private static readonly Encoding _gb18030Encoding;
    private static readonly Encoding _latin1Encoding;
    private static readonly UTF8Encoding _utf8Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static MapDataEditorWindowViewModel()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _gb18030Encoding = Encoding.GetEncoding("GB18030");
        _latin1Encoding = Encoding.GetEncoding("ISO-8859-1");
    }

    [ObservableProperty] private string _filePath = "";

    [ObservableProperty] private string _fileType = "";

    [ObservableProperty] private string _parserTypeName = "";

    [ObservableProperty] private string _parseStatus = "Not Loaded";

    [ObservableProperty] private Brush _parseStatusColor = Brushes.DimGray;

    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private ObservableCollection<MapDataAssetTreeNode> _assetTree = new();

    [ObservableProperty] private MapDataAssetTreeNode? _selectedAssetNode;

    [ObservableProperty] private MapDataAssetDetailModel _selectedAssetDetail = MapDataAssetDetailModel.Empty();

    private bool _hasSidesListAsset;
    private bool _hasPlayerScriptsListAsset;
    private int _playerCount;
    private int _scriptListCount;

    public bool LoadMapDataFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            MessageBox.Show("File does not exist: " + filePath);
            return false;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension is not (".map" or ".scb" or ".bin"))
        {
            MessageBox.Show("Unsupported file extension: " + extension);
            return false;
        }

        try
        {
            var context = LoadContext(filePath, extension, out var parserTypeName);

            FilePath = filePath;
            FileType = extension.TrimStart('.').ToUpperInvariant();
            ParserTypeName = parserTypeName;
            ParseStatus = "Parsed";
            ParseStatusColor = Brushes.LimeGreen;

            BuildCombinedTree(context);
            SelectedAssetNode = AssetTree.FirstOrDefault(n => n.Kind == MapDataAssetTreeNodeKind.ScriptRoot)
                                ?? AssetTree.FirstOrDefault();
            if (SelectedAssetNode is not null)
            {
                SelectedAssetNode.IsSelected = true;
            }

            return true;
        }
        catch (Exception ex)
        {
            ParseStatus = "Parse Failed";
            ParseStatusColor = Brushes.Crimson;
            ParserTypeName = "";
            AssetTree.Clear();
            SelectedAssetNode = null;
            _hasSidesListAsset = false;
            _hasPlayerScriptsListAsset = false;
            _playerCount = 0;
            _scriptListCount = 0;
            MessageBox.Show("Failed to load map data: " + ex.Message);
            return false;
        }
    }

    [RelayCommand]
    private void Closed()
    {
        // Per-window cleanup is currently not required.
    }

    [RelayCommand]
    private void AssetTreeItemSelectedChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedAssetNode = e.NewValue as MapDataAssetTreeNode;
    }

    partial void OnSelectedAssetNodeChanged(MapDataAssetTreeNode? value)
    {
        SelectedAssetDetail = BuildDetail(value);
    }

    private BaseContext LoadContext(string filePath, string extension, out string parserTypeName)
    {
        switch (extension)
        {
            case ".map":
            {
                var map = Ra3Map.Open(filePath);
                parserTypeName = nameof(Ra3Map);
                return map.Context;
            }
            case ".scb":
            {
                var mapScb = Ra3MapScb.FromFile(filePath);
                parserTypeName = nameof(Ra3MapScb);
                return mapScb.Context;
            }
            case ".bin":
            {
                if (TryLoadBinContext(filePath, out var context, out parserTypeName, out var error))
                {
                    return context;
                }

                throw new Exception(error);
            }
            default:
                throw new NotSupportedException("Unsupported file extension: " + extension);
        }
    }

    private void BuildCombinedTree(BaseContext context)
    {
        AssetTree.Clear();

        var sidesListAsset = FindSidesListAsset(context);
        var playerScriptsList = FindPlayerScriptsListAsset(context);

        _hasSidesListAsset = sidesListAsset is not null;
        _hasPlayerScriptsListAsset = playerScriptsList is not null;
        _playerCount = sidesListAsset?.PlayerDataList.Count ?? 0;
        _scriptListCount = playerScriptsList?.ScriptLists.Count ?? 0;

        AssetTree.Add(BuildPlayerRootNode(sidesListAsset));
        AssetTree.Add(BuildScriptRootNode(sidesListAsset, playerScriptsList));
    }

    private MapDataAssetTreeNode BuildPlayerRootNode(SidesListAsset? sidesListAsset)
    {
        var rootNode = new MapDataAssetTreeNode
        {
            DisplayName = PlayerRootDisplayName,
            Kind = MapDataAssetTreeNodeKind.PlayerRoot,
            IsExpanded = false
        };

        if (sidesListAsset is null)
        {
            return rootNode;
        }

        foreach (var player in sidesListAsset.PlayerDataList)
        {
            rootNode.Children.Add(new MapDataAssetTreeNode
            {
                DisplayName = SafeValue(() => player.Name),
                Kind = MapDataAssetTreeNodeKind.Player,
                Player = player
            });
        }

        return rootNode;
    }

    private MapDataAssetTreeNode BuildScriptRootNode(SidesListAsset? sidesListAsset, PlayerScriptsList? playerScriptsList)
    {
        var rootNode = new MapDataAssetTreeNode
        {
            DisplayName = ScriptRootDisplayName,
            Kind = MapDataAssetTreeNodeKind.ScriptRoot,
            IsExpanded = false
        };

        if (playerScriptsList is null)
        {
            return rootNode;
        }

        for (var i = 0; i < playerScriptsList.ScriptLists.Count; i++)
        {
            var scriptList = playerScriptsList.ScriptLists[i];
            var scriptListNode = new MapDataAssetTreeNode
            {
                DisplayName = ResolveScriptListDisplayName(sidesListAsset, i),
                Kind = MapDataAssetTreeNodeKind.ScriptList,
                ScriptList = scriptList,
                ScriptListIndex = i
            };

            AppendScriptChildren(scriptListNode.Children, scriptList.ScriptGroups, scriptList.Scripts);
            rootNode.Children.Add(scriptListNode);
        }

        return rootNode;
    }

    private void AppendScriptChildren(
        ObservableCollection<MapDataAssetTreeNode> target,
        IEnumerable<ScriptGroup> scriptGroups,
        IEnumerable<Script> scripts)
    {
        foreach (var scriptGroup in scriptGroups)
        {
            var scriptGroupNode = new MapDataAssetTreeNode
            {
                DisplayName = SafeValue(() => scriptGroup.Name),
                Kind = MapDataAssetTreeNodeKind.ScriptGroup,
                ScriptGroup = scriptGroup
            };

            AppendScriptChildren(scriptGroupNode.Children, scriptGroup.ScriptGroups, scriptGroup.Scripts);
            target.Add(scriptGroupNode);
        }

        foreach (var script in scripts)
        {
            var scriptNode = new MapDataAssetTreeNode
            {
                DisplayName = SafeValue(() => script.Name),
                Kind = MapDataAssetTreeNodeKind.Script,
                Script = script,
                IsExpanded = false
            };

            AppendScriptExecutionChildren(scriptNode, script);
            target.Add(scriptNode);
        }
    }

    private void AppendScriptExecutionChildren(MapDataAssetTreeNode scriptNode, Script script)
    {
        scriptNode.Children.Add(BuildIfRootNode(script));
        scriptNode.Children.Add(BuildThenRootNode(script));
        scriptNode.Children.Add(BuildElseRootNode(script));
    }

    private MapDataAssetTreeNode BuildIfRootNode(Script script)
    {
        var ifRootNode = new MapDataAssetTreeNode
        {
            DisplayName = "If",
            Kind = MapDataAssetTreeNodeKind.ScriptIfRoot,
            IsExpanded = false
        };

        for (var i = 0; i < script.ScriptOrConditions.Count; i++)
        {
            var orCondition = script.ScriptOrConditions[i];
            var orNode = new MapDataAssetTreeNode
            {
                DisplayName = $"OR #{i}",
                Kind = MapDataAssetTreeNodeKind.ScriptOrCondition,
                OrCondition = orCondition,
                OrConditionIndex = i,
                IsExpanded = false
            };

            foreach (var condition in orCondition.Conditions)
            {
                orNode.Children.Add(new MapDataAssetTreeNode
                {
                    DisplayName = BuildConditionSummaryText(condition),
                    Kind = MapDataAssetTreeNodeKind.ScriptCondition,
                    ScriptCondition = condition
                });
            }

            ifRootNode.Children.Add(orNode);
        }

        return ifRootNode;
    }

    private MapDataAssetTreeNode BuildThenRootNode(Script script)
    {
        var thenRootNode = new MapDataAssetTreeNode
        {
            DisplayName = "Then",
            Kind = MapDataAssetTreeNodeKind.ScriptThenRoot,
            IsExpanded = false
        };

        foreach (var action in script.ScriptActionOnTrue)
        {
            thenRootNode.Children.Add(new MapDataAssetTreeNode
            {
                DisplayName = BuildActionSummaryText(action),
                Kind = MapDataAssetTreeNodeKind.ScriptActionTrue,
                ScriptAction = action
            });
        }

        return thenRootNode;
    }

    private MapDataAssetTreeNode BuildElseRootNode(Script script)
    {
        var elseRootNode = new MapDataAssetTreeNode
        {
            DisplayName = "Else",
            Kind = MapDataAssetTreeNodeKind.ScriptElseRoot,
            IsExpanded = false
        };

        foreach (var action in script.ScriptActionOnFalse)
        {
            elseRootNode.Children.Add(new MapDataAssetTreeNode
            {
                DisplayName = BuildActionSummaryText(action),
                Kind = MapDataAssetTreeNodeKind.ScriptActionFalse,
                ScriptAction = action
            });
        }

        return elseRootNode;
    }

    private static string BuildConditionSummaryText(ScriptConditionContent condition)
    {
        return BuildCallSummaryText(condition.ContentName, condition.Arguments);
    }

    private static string BuildActionSummaryText(ScriptAction action)
    {
        return BuildCallSummaryText(action.ContentName, action.Arguments);
    }

    private static string BuildCallSummaryText(string? contentName, IEnumerable<ScriptArgument> arguments)
    {
        var name = string.IsNullOrWhiteSpace(contentName) ? "-" : NormalizeDisplayText(contentName);
        var argumentText = BuildArgumentListText(arguments);
        if (string.IsNullOrEmpty(argumentText))
        {
            return $"{name}()";
        }

        return $"{name}({argumentText})";
    }

    private string ResolveScriptListDisplayName(SidesListAsset? sidesListAsset, int index)
    {
        if (sidesListAsset is not null && index >= 0 && index < sidesListAsset.PlayerDataList.Count)
        {
            var playerName = sidesListAsset.PlayerDataList[index].Name;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return NeutralPlayerDisplayName;
            }

            return NormalizeDisplayText(playerName);
        }

        return $"\u73a9\u5bb6#{index}";
    }

    private static SidesListAsset? FindSidesListAsset(BaseContext context)
    {
        if (context.AssetDict.TryGetValue(AssetNameConst.SidesList, out var sidesAsset) &&
            sidesAsset is SidesListAsset sidesListAsset)
        {
            return sidesListAsset;
        }

        return context.AssetDict.Values.OfType<SidesListAsset>().FirstOrDefault();
    }

    private static PlayerScriptsList? FindPlayerScriptsListAsset(BaseContext context)
    {
        if (context.AssetDict.TryGetValue(AssetNameConst.PlayerScriptsList, out var scriptsAsset) &&
            scriptsAsset is PlayerScriptsList playerScriptsList)
        {
            return playerScriptsList;
        }

        return context.AssetDict.Values.OfType<PlayerScriptsList>().FirstOrDefault();
    }

    private MapDataAssetDetailModel BuildDetail(MapDataAssetTreeNode? node)
    {
        if (node is null)
        {
            return MapDataAssetDetailModel.Empty();
        }

        return node.Kind switch
        {
            MapDataAssetTreeNodeKind.PlayerRoot => BuildPlayerRootDetail(node),
            MapDataAssetTreeNodeKind.Player => BuildPlayerDetail(node),
            MapDataAssetTreeNodeKind.ScriptRoot => BuildScriptRootDetail(node),
            MapDataAssetTreeNodeKind.ScriptList => BuildScriptListDetail(node),
            MapDataAssetTreeNodeKind.ScriptGroup => BuildScriptGroupDetail(node),
            MapDataAssetTreeNodeKind.Script => BuildScriptDetail(node),
            MapDataAssetTreeNodeKind.ScriptIfRoot => BuildScriptBranchRootDetail(node, "If"),
            MapDataAssetTreeNodeKind.ScriptThenRoot => BuildScriptBranchRootDetail(node, "Then"),
            MapDataAssetTreeNodeKind.ScriptElseRoot => BuildScriptBranchRootDetail(node, "Else"),
            MapDataAssetTreeNodeKind.ScriptOrCondition => BuildOrConditionDetail(node),
            MapDataAssetTreeNodeKind.ScriptCondition => BuildScriptConditionDetail(node),
            MapDataAssetTreeNodeKind.ScriptActionTrue => BuildScriptActionDetail(node, "Then"),
            MapDataAssetTreeNodeKind.ScriptActionFalse => BuildScriptActionDetail(node, "Else"),
            _ => MapDataAssetDetailModel.Empty()
        };
    }

    private MapDataAssetDetailModel BuildPlayerRootDetail(MapDataAssetTreeNode node)
    {
        var detail = new MapDataAssetDetailModel
        {
            Title = node.DisplayName,
            Description = _hasSidesListAsset
                ? "SidesListAsset found."
                : "SidesListAsset not found. Player list is empty."
        };

        AddField(detail, "NodeType", "PlayerRoot");
        AddField(detail, "PlayerCount", _playerCount.ToString());
        AddField(detail, "SidesListAsset", _hasSidesListAsset ? "Found" : "Missing");

        if (!_hasSidesListAsset)
        {
            AddField(detail, "Hint", "Current map does not contain SidesListAsset.");
        }

        return detail;
    }

    private MapDataAssetDetailModel BuildPlayerDetail(MapDataAssetTreeNode node)
    {
        var player = node.Player;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "PlayerData fields (read-only)"
        };

        if (player is null)
        {
            AddField(detail, "Hint", "Player data is empty.");
            return detail;
        }

        AddField(detail, "Name", SafeValue(() => player.Name));
        AddField(detail, "DisplayName", SafeValue(() => player.DisplayName));
        AddField(detail, "IsHuman", ToBoolText(player.IsHuman));
        AddField(detail, "Faction", SafeValue(() => player.Faction));
        AddField(detail, "AllyPlayerNames", ToListText(player.AllyPlayerNames));
        AddField(detail, "EnemyPlayerNames", ToListText(player.EnemyPlayerNames));

        AddField(detail, "Personality", ToText(player.Personality));
        AddField(detail, "FactionIcon", ToText(player.FactionIcon));

        AddField(detail, "BaseBuilder", ToDifficultyText(player.BaseBuilder));
        AddField(detail, "UnitBuilder", ToDifficultyText(player.UnitBuilder));
        AddField(detail, "TeamBuilder", ToDifficultyText(player.TeamBuilder));
        AddField(detail, "EconomyBuilder", ToDifficultyText(player.EconomyBuilder));
        AddField(detail, "WallBuilder", ToDifficultyText(player.WallBuilder));
        AddField(detail, "UnitUpgrader", ToDifficultyText(player.UnitUpgrader));
        AddField(detail, "ScienceUpgrader", ToDifficultyText(player.ScienceUpgrader));
        AddField(detail, "Tactical", ToDifficultyText(player.Tactical));
        AddField(detail, "OpeningMover", ToDifficultyText(player.OpeningMover));

        AddField(detail, "Color", ToText(player.Color));
        AddField(detail, "RadarColor", ToText(player.RadarColor));

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptRootDetail(MapDataAssetTreeNode node)
    {
        var detail = new MapDataAssetDetailModel
        {
            Title = node.DisplayName,
            Description = _hasPlayerScriptsListAsset
                ? "PlayerScriptsList found."
                : "PlayerScriptsList not found. Script tree is empty."
        };

        AddField(detail, "NodeType", "ScriptRoot");
        AddField(detail, "ScriptListCount", _scriptListCount.ToString());
        AddField(detail, "PlayerScriptsList", _hasPlayerScriptsListAsset ? "Found" : "Missing");

        if (!_hasPlayerScriptsListAsset)
        {
            AddField(detail, "Hint", "Current map does not contain PlayerScriptsList.");
        }

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptListDetail(MapDataAssetTreeNode node)
    {
        var scriptList = node.ScriptList;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "ScriptList fields (read-only)"
        };

        if (scriptList is null)
        {
            AddField(detail, "Hint", "ScriptList is empty.");
            return detail;
        }

        var groupCount = scriptList.ScriptGroups.Count;
        var scriptCount = scriptList.Scripts.Count;

        AddField(detail, "NodeType", "ScriptList");
        AddField(detail, "PlayerName", detail.Title);
        AddField(detail, "Index", node.ScriptListIndex?.ToString() ?? "-");
        AddField(detail, "ScriptGroupCount", groupCount.ToString());
        AddField(detail, "ScriptCount", scriptCount.ToString());
        AddField(detail, "ChildCount", (groupCount + scriptCount).ToString());

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptGroupDetail(MapDataAssetTreeNode node)
    {
        var scriptGroup = node.ScriptGroup;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "ScriptGroup fields (read-only)"
        };

        if (scriptGroup is null)
        {
            AddField(detail, "Hint", "ScriptGroup is empty.");
            return detail;
        }

        var groupCount = scriptGroup.ScriptGroups.Count;
        var scriptCount = scriptGroup.Scripts.Count;

        AddField(detail, "NodeType", "ScriptGroup");
        AddField(detail, "Name", SafeValue(() => scriptGroup.Name));
        AddField(detail, "IsActive", ToBoolText(scriptGroup.IsActive));
        AddField(detail, "IsSubroutine", ToBoolText(scriptGroup.IsSubroutine));
        AddField(detail, "ScriptGroupCount", groupCount.ToString());
        AddField(detail, "ScriptCount", scriptCount.ToString());
        AddField(detail, "ChildCount", (groupCount + scriptCount).ToString());

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptDetail(MapDataAssetTreeNode node)
    {
        var script = node.Script;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "Script fields (read-only)"
        };

        if (script is null)
        {
            AddField(detail, "Hint", "Script is empty.");
            return detail;
        }

        AddField(detail, "NodeType", "Script");
        AddField(detail, "Name", SafeValue(() => script.Name));
        AddField(detail, "Comment", SafeValue(() => script.Comment));
        AddField(detail, "ConditionComment", SafeValue(() => script.ConditionComment));
        AddField(detail, "ActionComment", SafeValue(() => script.ActionComment));

        AddField(detail, "IsActive", ToBoolText(script.IsActive));
        AddField(detail, "IsSubroutine", ToBoolText(script.IsSubroutine));
        AddField(detail, "DeactivateUponSuccess", ToBoolText(script.DeactivateUponSuccess));

        AddField(detail, "ActiveInEasy", ToBoolText(script.ActiveInEasy));
        AddField(detail, "ActiveInMedium", ToBoolText(script.ActiveInMedium));
        AddField(detail, "ActiveInHard", ToBoolText(script.ActiveInHard));

        AddField(detail, "EvaluationInterval", script.EvaluationInterval.ToString());
        AddField(detail, "ActionsFireSequentially", ToBoolText(script.ActionsFireSequentially));
        AddField(detail, "LoopActions", ToBoolText(script.LoopActions));
        AddField(detail, "LoopCount", script.LoopCount.ToString());
        AddField(detail, "SequentialTargetType", SafeValue(() => script.SequentialTargetType.ToString()));
        AddField(detail, "SequentialTargetName", SafeValue(() => script.SequentialTargetName));

        AddField(detail, "IfCount", script.ScriptOrConditions.Count.ToString());
        AddField(detail, "ThenCount", script.ScriptActionOnTrue.Count.ToString());
        AddField(detail, "ElseCount", script.ScriptActionOnFalse.Count.ToString());

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptBranchRootDetail(MapDataAssetTreeNode node, string branchName)
    {
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = $"{branchName} branch items (read-only)"
        };

        AddField(detail, "NodeType", $"{branchName}Root");
        AddField(detail, "ItemCount", node.Children.Count.ToString());
        return detail;
    }

    private MapDataAssetDetailModel BuildOrConditionDetail(MapDataAssetTreeNode node)
    {
        var orCondition = node.OrCondition;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "OrCondition fields (read-only)"
        };

        if (orCondition is null)
        {
            AddField(detail, "Hint", "OrCondition is empty.");
            return detail;
        }

        AddField(detail, "NodeType", "ScriptOrCondition");
        AddField(detail, "Index", node.OrConditionIndex?.ToString() ?? "-");
        AddField(detail, "ConditionCount", orCondition.Conditions.Count.ToString());
        return detail;
    }

    private MapDataAssetDetailModel BuildScriptConditionDetail(MapDataAssetTreeNode node)
    {
        var condition = node.ScriptCondition;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = "ScriptConditionContent fields (read-only)"
        };

        if (condition is null)
        {
            AddField(detail, "Hint", "Condition is empty.");
            return detail;
        }

        AddField(detail, "NodeType", "ScriptCondition");
        AddField(detail, "Name", SafeValue(() => condition.ContentName));
        AddField(detail, "启用", ToBoolText(condition.Enabled));
        AddField(detail, "反转", ToBoolText(condition.IsInverted));

        var declareModel = TryGetScriptDeclareModel(condition);
        AddScriptDeclareFields(detail, declareModel);
        AddArgumentFields(detail, condition.Arguments, declareModel?.Arguments);
        return detail;
    }

    private MapDataAssetDetailModel BuildScriptActionDetail(MapDataAssetTreeNode node, string branchName)
    {
        var action = node.ScriptAction;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = $"ScriptAction ({branchName}) fields (read-only)"
        };

        if (action is null)
        {
            AddField(detail, "Hint", "Action is empty.");
            return detail;
        }

        AddField(detail, "NodeType", node.Kind == MapDataAssetTreeNodeKind.ScriptActionFalse ? "ScriptActionFalse" : "ScriptAction");
        AddField(detail, "Branch", branchName);
        AddField(detail, "Name", SafeValue(() => action.ContentName));
        AddField(detail, "启用", ToBoolText(action.Enabled));

        var declareModel = TryGetScriptDeclareModel(action);
        AddScriptDeclareFields(detail, declareModel);
        AddArgumentFields(detail, action.Arguments, declareModel?.Arguments);
        return detail;
    }

    private static ScriptDeclareModel? TryGetScriptDeclareModel(ScriptConditionContent condition)
    {
        try
        {
            return condition.ScriptDeclareModel;
        }
        catch
        {
            return null;
        }
    }

    private static ScriptDeclareModel? TryGetScriptDeclareModel(ScriptAction action)
    {
        try
        {
            return action.ScriptDeclareModel;
        }
        catch
        {
            return null;
        }
    }

    private static void AddScriptDeclareFields(MapDataAssetDetailModel detail, ScriptDeclareModel? declareModel)
    {
        AddField(detail, "脚本编号", declareModel?.EditorNumber.ToString() ?? "-");
        AddField(detail, "英文注释", ToDisplayText(declareModel?.ScriptName));
        AddField(detail, "中文注释", ToDisplayText(declareModel?.ScriptTrans));
        AddField(detail, "注释", ToDisplayText(declareModel?.ScriptDesc));
        AddField(detail, "参数模板", ToDisplayText(declareModel?.ScriptArg));
    }

    private static void AddArgumentFields(
        MapDataAssetDetailModel detail,
        IEnumerable<ScriptArgument> runtimeArguments,
        IReadOnlyList<ArgumentModel>? declareArguments)
    {
        var runtimeArgumentList = runtimeArguments.ToList();
        var declaredCount = declareArguments?.Count ?? 0;
        var count = Math.Max(runtimeArgumentList.Count, declaredCount);
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var runtimeArgument = i < runtimeArgumentList.Count ? runtimeArgumentList[i] : null;
            var declareArgument = declareArguments is not null && i < declareArguments.Count ? declareArguments[i] : null;

            var group = new MapDataAssetDetailFieldItem
            {
                Label = $"参数[{i}]",
                IsGroup = true,
                IsExpanded = true
            };
            AddArgumentFieldItems(group.Children, runtimeArgument, declareArgument);
            detail.Fields.Add(group);
        }
    }

    private static void AddArgumentFieldItems(
        ObservableCollection<MapDataAssetDetailFieldItem> fields,
        ScriptArgument? runtimeArgument,
        ArgumentModel? declareArgument)
    {
        var realType = ToDisplayText(declareArgument?.RealType);
        var valueLabel = realType == "-" ? "参数值" : $"参数值({realType})";
        AddField(fields, valueLabel, runtimeArgument is null ? "-" : ToArgumentDisplayText(runtimeArgument));

        // AddField(fields, "参数注释", ToDisplayText(declareArgument?.ExampleData));
        // AddField(fields, "参数类型编号", declareArgument?.TypeNumber.ToString() ?? "-");
    }

    private static void AddField(MapDataAssetDetailModel detail, string label, string value)
    {
        AddField(detail.Fields, label, value);
    }

    private static void AddField(ObservableCollection<MapDataAssetDetailFieldItem> fields, string label, string value)
    {
        fields.Add(new MapDataAssetDetailFieldItem
        {
            Label = label,
            Value = string.IsNullOrWhiteSpace(value) ? "-" : value
        });
    }

    private static string SafeValue(Func<string?> valueFactory)
    {
        try
        {
            var value = valueFactory();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            return NormalizeDisplayText(value);
        }
        catch
        {
            return "-";
        }
    }

    private static string ToText(object? value)
    {
        if (value is null)
        {
            return "-";
        }

        return NormalizeDisplayText(value.ToString() ?? "-");
    }

    private static string ToBoolText(bool value)
    {
        return value ? "True" : "False";
    }

    private static string ToListText(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return "-";
        }

        var materialized = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(NormalizeDisplayText)
            .ToList();

        return materialized.Count == 0 ? "-" : string.Join(", ", materialized);
    }

    private static string ToDisplayText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return NormalizeDisplayText(value);
    }

    private static string BuildArgumentListText(IEnumerable<ScriptArgument> arguments)
    {
        var values = arguments.Select(ToArgumentDisplayText).ToList();
        if (values.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", values);
    }

    private static string ToArgumentDisplayText(ScriptArgument argument)
    {
        try
        {
            var value = argument.ToJsonNode();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            return NormalizeDisplayText(value);
        }
        catch
        {
            return "-";
        }
    }

    private static string ToDifficultyText(PlayerDifficultyConfig? config)
    {
        if (config is null)
        {
            return "-";
        }

        return $"Easy={config.Easy}, Normal={config.Normal}, Hard={config.Hard}, Brutal={config.Brutal}";
    }

    private static string NormalizeDisplayText(string input)
    {
        var text = input.Trim();
        if (text.Length == 0)
        {
            return "-";
        }

        var best = text;
        var bestScore = ScoreReadable(text);
        foreach (var candidate in BuildDecodeCandidates(text))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var normalizedCandidate = CleanupCandidate(candidate);
            if (normalizedCandidate.Length == 0)
            {
                continue;
            }

            var candidateScore = ScoreReadable(normalizedCandidate);
            if (candidateScore > bestScore + 1)
            {
                best = normalizedCandidate;
                bestScore = candidateScore;
            }
        }

        if (!LooksLikeMojibake(text))
        {
            return text;
        }

        return best;
    }

    private static IEnumerable<string> BuildDecodeCandidates(string input)
    {
        yield return TryTranscode(input, _gb18030Encoding, _utf8Strict);
        yield return TryTranscode(input, Encoding.UTF8, _gb18030Encoding);
        yield return TryTranscode(input, _gb18030Encoding, Encoding.Unicode);
        yield return TryTranscode(input, Encoding.Unicode, _gb18030Encoding);
        yield return TryTranscode(input, Encoding.UTF8, Encoding.Unicode);
        yield return TryTranscode(input, Encoding.Unicode, _utf8Strict);
        yield return TryTranscode(input, _latin1Encoding, _utf8Strict);
        yield return TryTranscode(input, Encoding.UTF8, _latin1Encoding);
    }

    private static string TryTranscode(string input, Encoding bytesEncoding, Encoding targetTextEncoding)
    {
        try
        {
            var bytes = bytesEncoding.GetBytes(input);
            return targetTextEncoding.GetString(bytes);
        }
        catch
        {
            return "";
        }
    }

    private static string CleanupCandidate(string value)
    {
        return value.Replace("\0", "").Trim();
    }

    private static bool LooksLikeMojibake(string input)
    {
        var hardMarkers = new[]
        {
            '鐜', '鍒', '楄', '宸', '鏈', '鏁', '銆', '绌', '鍔', '璇', '妗', '鎬', '瀛'
        };

        if (input.Any(ch => hardMarkers.Contains(ch)))
        {
            return true;
        }

        foreach (var ch in input)
        {
            if (ch == '\uFFFD' || ch == '\0')
            {
                return true;
            }

            if (ch >= '\uE000' && ch <= '\uF8FF')
            {
                return true;
            }

            if (ch >= '\u3100' && ch <= '\u312F')
            {
                return true;
            }

            if (char.IsControl(ch) && !char.IsWhiteSpace(ch))
            {
                return true;
            }
        }

        return false;
    }

    private static int ScoreReadable(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return int.MinValue;
        }

        var score = 0;
        foreach (var ch in text)
        {
            if (ch == '\uFFFD' || ch == '\0')
            {
                score -= 24;
                continue;
            }

            if (ch >= '\uE000' && ch <= '\uF8FF')
            {
                score -= 12;
                continue;
            }

            if (ch >= '\u3100' && ch <= '\u312F')
            {
                score -= 6;
                continue;
            }

            if (char.IsControl(ch) && !char.IsWhiteSpace(ch))
            {
                score -= 12;
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                score += 4;
                continue;
            }

            if (ch >= 0x4E00 && ch <= 0x9FFF)
            {
                score += 3;
                continue;
            }

            if (char.IsWhiteSpace(ch) || ch is '_' or '-' or '.' or ',' or ':' or ';' or '/' or '\\' or '(' or ')' or '[' or ']' or '"' or '\'')
            {
                score += 1;
                continue;
            }

            score -= 1;
        }

        var mojibakeTextMarkers = new[]
        {
            "鏂", "鐜", "鍒", "锛", "銆", "绗", "寮", "璇", "妗", "鍚", "锟", "鈥"
        };
        foreach (var marker in mojibakeTextMarkers)
        {
            if (text.Contains(marker, StringComparison.Ordinal))
            {
                score -= 8;
            }
        }

        return score;
    }

    private static bool TryLoadBinContext(string filePath, out BaseContext context, out string parserTypeName, out string error)
    {
        foreach (var typeName in new[]
                 {
                     "Dreamness.Ra3.Map.Parser.Core.ClipBoard.Ra3ClipBoard",
                     "Dreamness.RA3.Map.Parser.Core.ClipBoard.Ra3ClipBoard"
                 })
        {
            if (TryLoadContextByTypeName(typeName, filePath, out context, out parserTypeName, out error))
            {
                return true;
            }
        }

        foreach (var typeName in new[]
                 {
                     "Dreamness.Ra3.Map.Parser.Core.ClipBoard.Ra3MapClipboard",
                     "Dreamness.RA3.Map.Parser.Core.ClipBoard.Ra3MapClipboard"
                 })
        {
            if (TryLoadContextByTypeName(typeName, filePath, out context, out parserTypeName, out error))
            {
                return true;
            }
        }

        context = null!;
        parserTypeName = "";
        error = "No available .bin parser found (Ra3ClipBoard / Ra3MapClipboard).";
        return false;
    }

    private static bool TryLoadContextByTypeName(string typeName, string filePath, out BaseContext context, out string parserTypeName, out string error)
    {
        context = null!;
        parserTypeName = "";
        error = "";

        var parserType = typeof(Ra3Map).Assembly.GetType(typeName, throwOnError: false);
        if (parserType is null)
        {
            return false;
        }

        var fromFileMethod = parserType.GetMethod(
            "FromFile",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null);

        if (fromFileMethod is null)
        {
            error = $"Parser misses FromFile method: {typeName}";
            return false;
        }

        try
        {
            var parserInstance = fromFileMethod.Invoke(null, new object[] { filePath });
            if (parserInstance is null)
            {
                error = $"Parser returned null instance: {typeName}";
                return false;
            }

            var contextField = parserType.GetField("Context", BindingFlags.Public | BindingFlags.Instance);
            if (contextField?.GetValue(parserInstance) is BaseContext fieldContext)
            {
                context = fieldContext;
                parserTypeName = parserType.Name;
                return true;
            }

            var contextProperty = parserType.GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);
            if (contextProperty?.GetValue(parserInstance) is BaseContext propertyContext)
            {
                context = propertyContext;
                parserTypeName = parserType.Name;
                return true;
            }

            error = $"Unable to read Context from parser: {typeName}";
            return false;
        }
        catch (TargetInvocationException ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}

