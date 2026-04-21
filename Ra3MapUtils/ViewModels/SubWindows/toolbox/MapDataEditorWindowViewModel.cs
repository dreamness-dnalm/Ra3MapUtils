using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using Dreamness.Ra3.Map.Parser.Asset.Collection.Property;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Script;
using Dreamness.Ra3.Map.Parser.Core.Base;
using Dreamness.Ra3.Map.Parser.Core.Map;
using Dreamness.Ra3.Map.Parser.Util.Compress;
using Ookii.Dialogs.WinForms;
using Ra3MapUtils.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class MapDataEditorWindowViewModel : ObservableObject
{
    private const string PlayerRootDisplayName = "玩家列表";
    private const string ScriptRootDisplayName = "脚本";
    private const string NeutralPlayerDisplayName = "(neutral)";

    private static readonly Encoding _gb18030Encoding;
    private static readonly Encoding _latin1Encoding;
    private static readonly UTF8Encoding _utf8Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private BaseContext? _loadedContext;
    private Ra3Map? _loadedMap;
    private object? _loadedParserInstance;
    private Action? _nativeSaveAction;
    private string _loadedExtension = "";

    private SidesListAsset? _sidesListAsset;
    private PlayerScriptsList? _playerScriptsList;

    private bool _hasSidesListAsset;
    private bool _hasPlayerScriptsListAsset;
    private int _playerCount;
    private int _scriptListCount;

    private sealed class LoadedContextResult
    {
        public LoadedContextResult(BaseContext context, string parserTypeName, object? parserInstance, Action? nativeSaveAction)
        {
            Context = context;
            ParserTypeName = parserTypeName;
            ParserInstance = parserInstance;
            NativeSaveAction = nativeSaveAction;
        }

        public BaseContext Context { get; }

        public string ParserTypeName { get; }

        public object? ParserInstance { get; }

        public Action? NativeSaveAction { get; }
    }

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

    [ObservableProperty] private bool _isDirty = false;

    [ObservableProperty] private bool _canSave = false;

    [ObservableProperty] private bool _canReload = false;

    [ObservableProperty] private ObservableCollection<MapDataAssetTreeNode> _assetTree = new();

    [ObservableProperty] private MapDataAssetTreeNode? _selectedAssetNode;

    [ObservableProperty] private MapDataAssetDetailModel _selectedAssetDetail = MapDataAssetDetailModel.Empty();

    partial void OnSelectedAssetNodeChanged(MapDataAssetTreeNode? value)
    {
        SelectedAssetDetail = BuildDetail(value);
    }

    partial void OnIsDirtyChanged(bool value)
    {
        RefreshActionState();
    }

    partial void OnFilePathChanged(string value)
    {
        RefreshActionState();
    }

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
        if (extension is not (".map" or ".scb" or ".bin" or ".paste"))
        {
            MessageBox.Show("Unsupported file extension: " + extension);
            return false;
        }

        try
        {
            var loadResult = LoadContext(filePath, extension);

            FilePath = filePath;
            FileType = extension.TrimStart('.').ToUpperInvariant();
            ParserTypeName = loadResult.ParserTypeName;
            ParseStatus = "Parsed";
            ParseStatusColor = Brushes.LimeGreen;

            _loadedContext = loadResult.Context;
            _loadedParserInstance = loadResult.ParserInstance;
            _nativeSaveAction = loadResult.NativeSaveAction;
            _loadedExtension = extension;

            BuildCombinedTree(loadResult.Context);
            SelectDefaultNode();

            IsDirty = false;
            RefreshActionState();
            return true;
        }
        catch (Exception ex)
        {
            ParseStatus = "Parse Failed";
            ParseStatusColor = Brushes.Crimson;
            ParserTypeName = "";
            AssetTree.Clear();
            SelectedAssetNode = null;
            SelectedAssetDetail = MapDataAssetDetailModel.Empty();
            ResetLoadedState();
            RefreshActionState();
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

    [RelayCommand]
    private void CommitDetailField(MapDataAssetDetailFieldItem? field)
    {
        if (field?.CommitAction is null)
        {
            return;
        }

        try
        {
            field.CommitAction(field);
        }
        catch (Exception ex)
        {
            MessageBox.Show("提交修改失败: " + ex.Message);
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!CanSave || _loadedContext is null || string.IsNullOrWhiteSpace(FilePath))
        {
            return;
        }

        try
        {
            Exception? nativeSaveError = null;
            var nativeSaved = false;
            if (_nativeSaveAction is not null)
            {
                try
                {
                    _nativeSaveAction();
                    nativeSaved = true;
                }
                catch (Exception ex)
                {
                    nativeSaveError = ex;
                }
            }

            if (!nativeSaved)
            {
                try
                {
                    SaveContextToRawFile(_loadedContext, FilePath);
                }
                catch (Exception rawSaveError)
                {
                    if (nativeSaveError is not null)
                    {
                        throw new Exception(
                            "Native save failed: " + nativeSaveError.Message + "; fallback raw save failed: " + rawSaveError.Message,
                            rawSaveError);
                    }

                    throw;
                }
            }

            IsDirty = false;
            ParseStatus = "Saved";
            ParseStatusColor = Brushes.LimeGreen;
            RefreshActionState();
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败: " + ex.Message);
        }
    }

    [RelayCommand]
    private void Reload()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            return;
        }

        if (IsDirty)
        {
            var result = MessageBox.Show(
                "存在未保存修改，确定重新载入并放弃当前修改吗？",
                "确认",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }
        }

        LoadMapDataFromFile(FilePath);
    }

    [RelayCommand]
    private void AddScript(MapDataAssetTreeNode? targetNode)
    {
        if (_loadedContext is null)
        {
            return;
        }

        var node = ResolveTargetNode(targetNode);
        if (node is null)
        {
            return;
        }

        if (node.Kind == MapDataAssetTreeNodeKind.ScriptList && node.ScriptList is not null)
        {
            var newScriptName = BuildUniqueName(node.ScriptList.Scripts.Select(s => s.Name), "新脚本");
            var newScript = CreateDefaultScript(newScriptName);
            node.ScriptList.Scripts.Add(newScript);
            MarkDirtyAndRefresh(node, newScript, MapDataAssetTreeNodeKind.Script);
            return;
        }

        if (node.Kind == MapDataAssetTreeNodeKind.ScriptGroup && node.ScriptGroup is not null)
        {
            var newScriptName = BuildUniqueName(node.ScriptGroup.Scripts.Select(s => s.Name), "新脚本");
            var newScript = CreateDefaultScript(newScriptName);
            node.ScriptGroup.Scripts.Add(newScript);
            MarkDirtyAndRefresh(node, newScript, MapDataAssetTreeNodeKind.Script);
        }
    }

    [RelayCommand]
    private void DeleteScript(MapDataAssetTreeNode? targetNode)
    {
        var node = ResolveTargetNode(targetNode);
        if (node?.Script is null || _playerScriptsList is null)
        {
            return;
        }

        var result = MessageBox.Show(
            "确定删除该脚本吗？",
            "确认",
            System.Windows.Forms.MessageBoxButtons.YesNo,
            System.Windows.Forms.MessageBoxIcon.Warning);
        if (result != System.Windows.Forms.DialogResult.Yes)
        {
            return;
        }

        var parentNode = FindParentNode(node);
        if (!RemoveScriptRecursive(_playerScriptsList.ScriptLists, node.Script))
        {
            return;
        }

        var preferredSelection = ResolvePreferredSelection(parentNode);
        MarkDirtyAndRefresh(parentNode, preferredSelection.Entity, preferredSelection.Kind);
    }

    [RelayCommand]
    private void RenameScript(MapDataAssetTreeNode? targetNode)
    {
        var node = ResolveTargetNode(targetNode);
        if (node?.Script is null)
        {
            return;
        }

        var input = PromptName("重命名脚本", "请输入脚本名", node.Script.Name);
        if (input is null)
        {
            return;
        }

        node.Script.Name = input;
        MarkDirtyAndRefresh(node, node.Script, MapDataAssetTreeNodeKind.Script);
    }

    [RelayCommand]
    private void AddScriptGroup(MapDataAssetTreeNode? targetNode)
    {
        if (_loadedContext is null)
        {
            return;
        }

        var node = ResolveTargetNode(targetNode);
        if (node is null)
        {
            return;
        }

        if (node.Kind == MapDataAssetTreeNodeKind.ScriptList && node.ScriptList is not null)
        {
            var newGroupName = BuildUniqueName(node.ScriptList.ScriptGroups.Select(g => g.Name), "新文件夹");
            var newGroup = ScriptGroup.Empty(newGroupName, true, false, _loadedContext);
            node.ScriptList.ScriptGroups.Add(newGroup);
            MarkDirtyAndRefresh(node, newGroup, MapDataAssetTreeNodeKind.ScriptGroup);
            return;
        }

        if (node.Kind == MapDataAssetTreeNodeKind.ScriptGroup && node.ScriptGroup is not null)
        {
            var newGroupName = BuildUniqueName(node.ScriptGroup.ScriptGroups.Select(g => g.Name), "新文件夹");
            var newGroup = ScriptGroup.Empty(newGroupName, true, false, _loadedContext);
            node.ScriptGroup.ScriptGroups.Add(newGroup);
            MarkDirtyAndRefresh(node, newGroup, MapDataAssetTreeNodeKind.ScriptGroup);
        }
    }

    [RelayCommand]
    private void DeleteScriptGroup(MapDataAssetTreeNode? targetNode)
    {
        var node = ResolveTargetNode(targetNode);
        if (node?.ScriptGroup is null || _playerScriptsList is null)
        {
            return;
        }

        var result = MessageBox.Show(
            "确定删除该文件夹吗？",
            "确认",
            System.Windows.Forms.MessageBoxButtons.YesNo,
            System.Windows.Forms.MessageBoxIcon.Warning);
        if (result != System.Windows.Forms.DialogResult.Yes)
        {
            return;
        }

        var parentNode = FindParentNode(node);
        if (!RemoveScriptGroupRecursive(_playerScriptsList.ScriptLists, node.ScriptGroup))
        {
            return;
        }

        var preferredSelection = ResolvePreferredSelection(parentNode);
        MarkDirtyAndRefresh(parentNode, preferredSelection.Entity, preferredSelection.Kind);
    }

    [RelayCommand]
    private void RenameScriptGroup(MapDataAssetTreeNode? targetNode)
    {
        var node = ResolveTargetNode(targetNode);
        if (node?.ScriptGroup is null)
        {
            return;
        }

        var input = PromptName("重命名文件夹", "请输入文件夹名", node.ScriptGroup.Name);
        if (input is null)
        {
            return;
        }

        node.ScriptGroup.Name = input;
        MarkDirtyAndRefresh(node, node.ScriptGroup, MapDataAssetTreeNodeKind.ScriptGroup);
    }

    private MapDataAssetTreeNode? ResolveTargetNode(MapDataAssetTreeNode? node)
    {
        return node ?? SelectedAssetNode;
    }

    private void ResetLoadedState()
    {
        _loadedContext = null;
        _loadedMap = null;
        _loadedParserInstance = null;
        _nativeSaveAction = null;
        _loadedExtension = "";
        _sidesListAsset = null;
        _playerScriptsList = null;
        _hasSidesListAsset = false;
        _hasPlayerScriptsListAsset = false;
        _playerCount = 0;
        _scriptListCount = 0;
        IsDirty = false;
    }

    private void RefreshActionState()
    {
        CanReload = !string.IsNullOrWhiteSpace(FilePath) && _loadedContext is not null;
        CanSave = CanReload && IsDirty;
    }

    private LoadedContextResult LoadContext(string filePath, string extension)
    {
        _loadedMap = null;

        switch (extension)
        {
            case ".map":
            {
                _loadedMap = Ra3Map.Open(filePath);
                return new LoadedContextResult(_loadedMap.Context, nameof(Ra3Map), _loadedMap, () => _loadedMap.Save());
            }
            case ".scb":
            {
                var mapScb = Ra3MapScb.FromFile(filePath);
                return new LoadedContextResult(mapScb.Context, nameof(Ra3MapScb), mapScb, () => mapScb.Save());
            }
            case ".bin":
            case ".paste":
            {
                if (TryLoadBinContext(filePath, out var loadResult, out var error))
                {
                    return loadResult;
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

        _sidesListAsset = FindSidesListAsset(context);
        _playerScriptsList = FindPlayerScriptsListAsset(context);

        _hasSidesListAsset = _sidesListAsset is not null;
        _hasPlayerScriptsListAsset = _playerScriptsList is not null;
        _playerCount = _sidesListAsset?.PlayerDataList.Count ?? 0;
        _scriptListCount = _playerScriptsList?.ScriptLists.Count ?? 0;

        AssetTree.Add(BuildPlayerRootNode(_sidesListAsset));
        AssetTree.Add(BuildScriptRootNode(_sidesListAsset, _playerScriptsList));
    }

    private void SelectDefaultNode()
    {
        var defaultNode = AssetTree.FirstOrDefault(n => n.Kind == MapDataAssetTreeNodeKind.ScriptRoot)
                          ?? AssetTree.FirstOrDefault();
        SetSelectedNode(defaultNode);
    }

    private void SetSelectedNode(MapDataAssetTreeNode? node)
    {
        SelectedAssetNode = node;
        if (node is not null)
        {
            node.IsSelected = true;
        }
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

        return $"玩家#{index}";
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
            Description = "ScriptConditionContent fields"
        };

        if (condition is null)
        {
            AddField(detail, "Hint", "Condition is empty.");
            return detail;
        }

        AddField(detail, "NodeType", "ScriptCondition");

        AddEditableComboField(
            detail.Fields,
            "Name",
            condition.ContentName,
            ScriptData.ConditionDict.Keys.OrderBy(k => k),
            field =>
            {
                var newName = (field.Value ?? "").Trim();
                if (string.Equals(newName, condition.ContentName, StringComparison.Ordinal))
                {
                    return;
                }

                if (!TryApplyConditionNameChange(condition, newName, out var error))
                {
                    MessageBox.Show(error);
                    field.Value = condition.ContentName;
                    return;
                }

                MarkDirtyAndRefresh(node, condition, MapDataAssetTreeNodeKind.ScriptCondition);
            });

        AddEditableCheckField(
            detail.Fields,
            "启用",
            condition.Enabled,
            field =>
            {
                if (condition.Enabled == field.BoolValue)
                {
                    return;
                }

                condition.Enabled = field.BoolValue;
                MarkDirtyAndRefresh(node, condition, MapDataAssetTreeNodeKind.ScriptCondition);
            });

        AddField(detail, "反转", ToBoolText(condition.IsInverted));

        var declareModel = TryGetScriptDeclareModel(condition);
        AddScriptDeclareFields(detail, declareModel);
        AddArgumentFields(detail, node, condition.Arguments, declareModel?.Arguments);

        return detail;
    }

    private MapDataAssetDetailModel BuildScriptActionDetail(MapDataAssetTreeNode node, string branchName)
    {
        var action = node.ScriptAction;
        var detail = new MapDataAssetDetailModel
        {
            Title = string.IsNullOrWhiteSpace(node.DisplayName) ? "-" : node.DisplayName,
            Description = $"ScriptAction ({branchName}) fields"
        };

        if (action is null)
        {
            AddField(detail, "Hint", "Action is empty.");
            return detail;
        }

        AddField(detail, "NodeType", node.Kind == MapDataAssetTreeNodeKind.ScriptActionFalse ? "ScriptActionFalse" : "ScriptAction");
        AddField(detail, "Branch", branchName);

        AddEditableComboField(
            detail.Fields,
            "Name",
            action.ContentName,
            ScriptData.ActionDict.Keys.OrderBy(k => k),
            field =>
            {
                var newName = (field.Value ?? "").Trim();
                if (string.Equals(newName, action.ContentName, StringComparison.Ordinal))
                {
                    return;
                }

                if (!TryApplyActionNameChange(action, newName, out var error))
                {
                    MessageBox.Show(error);
                    field.Value = action.ContentName;
                    return;
                }

                MarkDirtyAndRefresh(node, action, node.Kind);
            });

        AddEditableCheckField(
            detail.Fields,
            "启用",
            action.Enabled,
            field =>
            {
                if (action.Enabled == field.BoolValue)
                {
                    return;
                }

                action.Enabled = field.BoolValue;
                MarkDirtyAndRefresh(node, action, node.Kind);
            });

        var declareModel = TryGetScriptDeclareModel(action);
        AddScriptDeclareFields(detail, declareModel);
        AddArgumentFields(detail, node, action.Arguments, declareModel?.Arguments);

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

    private void AddArgumentFields(
        MapDataAssetDetailModel detail,
        MapDataAssetTreeNode node,
        IEnumerable<ScriptArgument> runtimeArguments,
        IEnumerable<ArgumentModel>? declareArguments)
    {
        var runtimeArgumentList = runtimeArguments.ToList();
        var declaredArgumentList = declareArguments?.ToList() ?? new List<ArgumentModel>();
        var count = Math.Max(runtimeArgumentList.Count, declaredArgumentList.Count);
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var runtimeArgument = i < runtimeArgumentList.Count ? runtimeArgumentList[i] : null;
            var declareArgument = i < declaredArgumentList.Count ? declaredArgumentList[i] : null;

            var group = new MapDataAssetDetailFieldItem
            {
                Label = $"参数[{i}]",
                IsGroup = true,
                IsExpanded = true
            };

            AddArgumentFieldItems(group.Children, node, runtimeArgument, declareArgument);
            detail.Fields.Add(group);
        }
    }

    private void AddArgumentFieldItems(
        ObservableCollection<MapDataAssetDetailFieldItem> fields,
        MapDataAssetTreeNode node,
        ScriptArgument? runtimeArgument,
        ArgumentModel? declareArgument)
    {
        var realType = ToDisplayText(declareArgument?.RealType ?? runtimeArgument?.ArgumentModel.RealType);
        var valueLabel = realType == "-" ? "参数值" : $"参数值({realType})";

        if (runtimeArgument is null)
        {
            AddField(fields, valueLabel, "-");
            return;
        }

        var currentDisplay = ToArgumentDisplayText(runtimeArgument);

        if (realType is "String" or "Int32" or "Double")
        {
            AddEditableTextField(
                fields,
                valueLabel,
                currentDisplay,
                field =>
                {
                    if (!TryApplyArgumentValue(runtimeArgument, declareArgument, field.Value ?? "", out var error))
                    {
                        MessageBox.Show(error);
                        field.Value = ToArgumentDisplayText(runtimeArgument);
                        return;
                    }

                    MarkDirtyAndRefresh(node, runtimeArgument, node.Kind);
                });
        }
        else
        {
            AddField(fields, valueLabel, currentDisplay);
        }

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
            Value = string.IsNullOrWhiteSpace(value) ? "-" : value,
            EditorType = MapDataAssetDetailFieldEditorType.ReadOnly
        });
    }

    private static void AddEditableTextField(
        ObservableCollection<MapDataAssetDetailFieldItem> fields,
        string label,
        string value,
        Action<MapDataAssetDetailFieldItem> commitAction)
    {
        fields.Add(new MapDataAssetDetailFieldItem
        {
            Label = label,
            Value = string.IsNullOrWhiteSpace(value) ? "-" : value,
            EditorType = MapDataAssetDetailFieldEditorType.TextBox,
            CommitAction = commitAction
        });
    }

    private static void AddEditableCheckField(
        ObservableCollection<MapDataAssetDetailFieldItem> fields,
        string label,
        bool value,
        Action<MapDataAssetDetailFieldItem> commitAction)
    {
        fields.Add(new MapDataAssetDetailFieldItem
        {
            Label = label,
            BoolValue = value,
            EditorType = MapDataAssetDetailFieldEditorType.CheckBox,
            CommitAction = commitAction
        });
    }

    private static void AddEditableComboField(
        ObservableCollection<MapDataAssetDetailFieldItem> fields,
        string label,
        string selectedValue,
        IEnumerable<string> options,
        Action<MapDataAssetDetailFieldItem> commitAction)
    {
        var item = new MapDataAssetDetailFieldItem
        {
            Label = label,
            Value = selectedValue,
            EditorType = MapDataAssetDetailFieldEditorType.ComboBox,
            CommitAction = commitAction
        };

        foreach (var option in options)
        {
            item.Options.Add(option);
        }

        if (!item.Options.Any(o => string.Equals(o, selectedValue, StringComparison.Ordinal)))
        {
            item.Options.Insert(0, selectedValue);
        }

        fields.Add(item);
    }

    private bool TryApplyConditionNameChange(ScriptConditionContent condition, string newName, out string error)
    {
        error = "";

        if (_loadedContext is null)
        {
            error = "当前上下文为空，无法修改命令名。";
            return false;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            error = "命令名不能为空。";
            return false;
        }

        if (!ScriptData.ConditionDict.TryGetValue(newName, out var declareModel))
        {
            error = "未找到对应条件命令声明: " + newName;
            return false;
        }

        condition.SetContentName(newName, _loadedContext);
        condition.ContentType = declareModel.EditorNumber;
        condition.AssetPropertyType = AssetProperty.AssetPropertyType.stringType;

        condition.Arguments.Clear();
        foreach (var argumentModel in declareModel.Arguments)
        {
            condition.Arguments.Add(CreateDefaultArgument(argumentModel, false));
        }

        condition.MarkModified();
        return true;
    }

    private bool TryApplyActionNameChange(ScriptAction action, string newName, out string error)
    {
        error = "";

        if (_loadedContext is null)
        {
            error = "当前上下文为空，无法修改命令名。";
            return false;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            error = "命令名不能为空。";
            return false;
        }

        if (!ScriptData.ActionDict.TryGetValue(newName, out var declareModel))
        {
            error = "未找到对应动作命令声明: " + newName;
            return false;
        }

        action.SetContentName(newName, _loadedContext);
        action.ContentType = declareModel.EditorNumber;
        action.AssetPropertyType = AssetProperty.AssetPropertyType.stringType;

        action.Arguments.Clear();
        foreach (var argumentModel in declareModel.Arguments)
        {
            action.Arguments.Add(CreateDefaultArgument(argumentModel, newName == "DEBUG_MESSAGE_BOX"));
        }

        action.MarkModified();
        return true;
    }

    private static bool TryApplyArgumentValue(
        ScriptArgument runtimeArgument,
        ArgumentModel? declareArgument,
        string rawInput,
        out string error)
    {
        var realType = declareArgument?.RealType ?? runtimeArgument.ArgumentModel.RealType;

        switch (realType)
        {
            case "String":
                runtimeArgument.StringValue = rawInput;
                error = "";
                return true;
            case "Int32":
                if (int.TryParse(rawInput, out var intValue))
                {
                    runtimeArgument.IntValue = intValue;
                    error = "";
                    return true;
                }

                error = "参数值必须是 Int32 整数。";
                return false;
            case "Double":
                if (float.TryParse(rawInput, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue) ||
                    float.TryParse(rawInput, NumberStyles.Float, CultureInfo.CurrentCulture, out floatValue))
                {
                    runtimeArgument.FloatValue = floatValue;
                    error = "";
                    return true;
                }

                error = "参数值必须是 Double 浮点数。";
                return false;
            default:
                error = "当前参数类型暂不支持编辑: " + realType;
                return false;
        }
    }

    private static ScriptArgument CreateDefaultArgument(ArgumentModel argumentModel, bool requireUtf8)
    {
        var defaultValue = argumentModel.RealType switch
        {
            "String" => "",
            "Int32" => "0",
            "Double" => "0",
            "Vec3D" => "0,0,0",
            _ => ""
        };

        return ScriptArgument.Of(argumentModel, defaultValue, requireUtf8);
    }

    private Script CreateDefaultScript(string name)
    {
        if (_loadedContext is null)
        {
            throw new InvalidOperationException("Context is not loaded.");
        }

        var script = Script.Default(name, _loadedContext);
        var orCondition = OrCondition.Empty(_loadedContext);
        orCondition.Conditions.Add(ScriptConditionContent.Of("CONDITION_TRUE", new List<string>(), _loadedContext));
        script.ScriptOrConditions.Add(orCondition);
        return script;
    }

    private static string BuildUniqueName(IEnumerable<string> existingNames, string baseName)
    {
        var nameSet = new HashSet<string>(
            existingNames.Where(n => !string.IsNullOrWhiteSpace(n)),
            StringComparer.OrdinalIgnoreCase);

        if (!nameSet.Contains(baseName))
        {
            return baseName;
        }

        var index = 1;
        while (nameSet.Contains($"{baseName}_{index}"))
        {
            index++;
        }

        return $"{baseName}_{index}";
    }

    private static bool RemoveScriptRecursive(IEnumerable<ScriptList> scriptLists, Script script)
    {
        foreach (var scriptList in scriptLists)
        {
            if (scriptList.Scripts.Contains(script))
            {
                scriptList.Scripts.Remove(script);
                return true;
            }

            if (RemoveScriptRecursive(scriptList.ScriptGroups, script))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RemoveScriptRecursive(IEnumerable<ScriptGroup> scriptGroups, Script script)
    {
        foreach (var scriptGroup in scriptGroups)
        {
            if (scriptGroup.Scripts.Contains(script))
            {
                scriptGroup.Scripts.Remove(script);
                return true;
            }

            if (RemoveScriptRecursive(scriptGroup.ScriptGroups, script))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RemoveScriptGroupRecursive(IEnumerable<ScriptList> scriptLists, ScriptGroup scriptGroup)
    {
        foreach (var scriptList in scriptLists)
        {
            if (scriptList.ScriptGroups.Contains(scriptGroup))
            {
                scriptList.ScriptGroups.Remove(scriptGroup);
                return true;
            }

            if (RemoveScriptGroupRecursive(scriptList.ScriptGroups, scriptGroup))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RemoveScriptGroupRecursive(IEnumerable<ScriptGroup> scriptGroups, ScriptGroup scriptGroup)
    {
        foreach (var group in scriptGroups)
        {
            if (group.ScriptGroups.Contains(scriptGroup))
            {
                group.ScriptGroups.Remove(scriptGroup);
                return true;
            }

            if (RemoveScriptGroupRecursive(group.ScriptGroups, scriptGroup))
            {
                return true;
            }
        }

        return false;
    }

    private static string? PromptName(string title, string instruction, string defaultValue)
    {
        var inputDialog = new InputDialog
        {
            WindowTitle = title,
            MainInstruction = instruction,
            Content = instruction,
            Input = defaultValue
        };

        if (inputDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return null;
        }

        var result = inputDialog.Input?.Trim() ?? "";
        if (result == "")
        {
            MessageBox.Show("名称不能为空");
            return null;
        }

        return result;
    }

    private MapDataAssetTreeNode? FindParentNode(MapDataAssetTreeNode target)
    {
        foreach (var root in AssetTree)
        {
            var parent = FindParentNode(root, target);
            if (parent is not null)
            {
                return parent;
            }
        }

        return null;
    }

    private static MapDataAssetTreeNode? FindParentNode(MapDataAssetTreeNode current, MapDataAssetTreeNode target)
    {
        foreach (var child in current.Children)
        {
            if (ReferenceEquals(child, target))
            {
                return current;
            }

            var parent = FindParentNode(child, target);
            if (parent is not null)
            {
                return parent;
            }
        }

        return null;
    }

    private static (object? Entity, MapDataAssetTreeNodeKind? Kind) ResolvePreferredSelection(MapDataAssetTreeNode? node)
    {
        if (node is null)
        {
            return (null, null);
        }

        return node.Kind switch
        {
            MapDataAssetTreeNodeKind.Player => (node.Player, node.Kind),
            MapDataAssetTreeNodeKind.ScriptList => (node.ScriptList, node.Kind),
            MapDataAssetTreeNodeKind.ScriptGroup => (node.ScriptGroup, node.Kind),
            MapDataAssetTreeNodeKind.Script => (node.Script, node.Kind),
            MapDataAssetTreeNodeKind.ScriptOrCondition => (node.OrCondition, node.Kind),
            MapDataAssetTreeNodeKind.ScriptCondition => (node.ScriptCondition, node.Kind),
            MapDataAssetTreeNodeKind.ScriptActionTrue or MapDataAssetTreeNodeKind.ScriptActionFalse => (node.ScriptAction, node.Kind),
            _ => (null, node.Kind)
        };
    }

    private void MarkDirtyAndRefresh(MapDataAssetTreeNode? previousNode, object? preferredEntity, MapDataAssetTreeNodeKind? preferredKind)
    {
        if (_loadedContext is null)
        {
            return;
        }

        IsDirty = true;
        RebuildTreeAndReselect(previousNode, preferredEntity, preferredKind);
    }

    private void RebuildTreeAndReselect(
        MapDataAssetTreeNode? previousNode,
        object? preferredEntity,
        MapDataAssetTreeNodeKind? preferredKind)
    {
        if (_loadedContext is null)
        {
            return;
        }

        var previousNodeSnapshot = previousNode ?? SelectedAssetNode;

        BuildCombinedTree(_loadedContext);

        MapDataAssetTreeNode? targetNode = null;

        if (preferredEntity is not null && preferredKind is not null)
        {
            targetNode = EnumerateNodes(AssetTree)
                .FirstOrDefault(node => node.Kind == preferredKind && NodeEntityReferenceEquals(node, preferredEntity));
        }

        if (targetNode is null && previousNodeSnapshot is not null)
        {
            targetNode = FindNodeBySnapshot(previousNodeSnapshot);
        }

        if (targetNode is null)
        {
            targetNode = AssetTree.FirstOrDefault(n => n.Kind == MapDataAssetTreeNodeKind.ScriptRoot)
                         ?? AssetTree.FirstOrDefault();
        }

        SetSelectedNode(targetNode);
    }

    private MapDataAssetTreeNode? FindNodeBySnapshot(MapDataAssetTreeNode snapshot)
    {
        return EnumerateNodes(AssetTree).FirstOrDefault(node => NodeSnapshotMatches(node, snapshot));
    }

    private static IEnumerable<MapDataAssetTreeNode> EnumerateNodes(IEnumerable<MapDataAssetTreeNode> roots)
    {
        foreach (var root in roots)
        {
            yield return root;
            foreach (var child in EnumerateNodes(root.Children))
            {
                yield return child;
            }
        }
    }

    private static bool NodeSnapshotMatches(MapDataAssetTreeNode current, MapDataAssetTreeNode snapshot)
    {
        if (current.Kind != snapshot.Kind)
        {
            return false;
        }

        return current.Kind switch
        {
            MapDataAssetTreeNodeKind.PlayerRoot or
            MapDataAssetTreeNodeKind.ScriptRoot or
            MapDataAssetTreeNodeKind.ScriptIfRoot or
            MapDataAssetTreeNodeKind.ScriptThenRoot or
            MapDataAssetTreeNodeKind.ScriptElseRoot => true,
            MapDataAssetTreeNodeKind.Player => ReferenceEquals(current.Player, snapshot.Player),
            MapDataAssetTreeNodeKind.ScriptList => ReferenceEquals(current.ScriptList, snapshot.ScriptList),
            MapDataAssetTreeNodeKind.ScriptGroup => ReferenceEquals(current.ScriptGroup, snapshot.ScriptGroup),
            MapDataAssetTreeNodeKind.Script => ReferenceEquals(current.Script, snapshot.Script),
            MapDataAssetTreeNodeKind.ScriptOrCondition => ReferenceEquals(current.OrCondition, snapshot.OrCondition),
            MapDataAssetTreeNodeKind.ScriptCondition => ReferenceEquals(current.ScriptCondition, snapshot.ScriptCondition),
            MapDataAssetTreeNodeKind.ScriptActionTrue or MapDataAssetTreeNodeKind.ScriptActionFalse =>
                ReferenceEquals(current.ScriptAction, snapshot.ScriptAction),
            _ => false
        };
    }

    private static bool NodeEntityReferenceEquals(MapDataAssetTreeNode node, object entity)
    {
        return node.Kind switch
        {
            MapDataAssetTreeNodeKind.Player => ReferenceEquals(node.Player, entity),
            MapDataAssetTreeNodeKind.ScriptList => ReferenceEquals(node.ScriptList, entity),
            MapDataAssetTreeNodeKind.ScriptGroup => ReferenceEquals(node.ScriptGroup, entity),
            MapDataAssetTreeNodeKind.Script => ReferenceEquals(node.Script, entity),
            MapDataAssetTreeNodeKind.ScriptOrCondition => ReferenceEquals(node.OrCondition, entity),
            MapDataAssetTreeNodeKind.ScriptCondition => ReferenceEquals(node.ScriptCondition, entity),
            MapDataAssetTreeNodeKind.ScriptActionTrue or MapDataAssetTreeNodeKind.ScriptActionFalse => ReferenceEquals(node.ScriptAction, entity),
            _ => false
        };
    }

    private static void SaveContextToRawFile(BaseContext context, string filePath)
    {
        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath) && !Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        using var stream = File.Create(filePath);
        using var binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(CompressConst.UnCompressFlag);
        binaryWriter.Write(context.ToBytes());
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

    private static bool TryLoadBinContext(string filePath, out LoadedContextResult loadResult, out string error)
    {
        foreach (var typeName in new[]
                 {
                     "Dreamness.Ra3.Map.Parser.Core.ClipBoard.Ra3ClipBoard",
                     "Dreamness.RA3.Map.Parser.Core.ClipBoard.Ra3ClipBoard"
                 })
        {
            if (TryLoadContextByTypeName(typeName, filePath, out loadResult, out error))
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
            if (TryLoadContextByTypeName(typeName, filePath, out loadResult, out error))
            {
                return true;
            }
        }

        loadResult = null!;
        error = "No available clipboard parser found (Ra3ClipBoard / Ra3MapClipboard).";
        return false;
    }

    private static bool TryLoadContextByTypeName(string typeName, string filePath, out LoadedContextResult loadResult, out string error)
    {
        loadResult = null!;
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
                loadResult = new LoadedContextResult(
                    fieldContext,
                    parserType.Name,
                    parserInstance,
                    BuildNativeSaveAction(parserType, parserInstance, filePath));
                return true;
            }

            var contextProperty = parserType.GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);
            if (contextProperty?.GetValue(parserInstance) is BaseContext propertyContext)
            {
                loadResult = new LoadedContextResult(
                    propertyContext,
                    parserType.Name,
                    parserInstance,
                    BuildNativeSaveAction(parserType, parserInstance, filePath));
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

    private static Action? BuildNativeSaveAction(Type parserType, object parserInstance, string filePath)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        var saveNoArgMethod = parserType.GetMethod(
            "Save",
            flags,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);
        if (saveNoArgMethod is not null)
        {
            return () => saveNoArgMethod.Invoke(parserInstance, Array.Empty<object>());
        }

        var saveBoolMethod = parserType.GetMethod(
            "Save",
            flags,
            binder: null,
            types: new[] { typeof(bool) },
            modifiers: null);
        if (saveBoolMethod is not null)
        {
            return () => saveBoolMethod.Invoke(parserInstance, new object[] { false });
        }

        var saveAsMethod = parserType.GetMethod(
            "SaveAs",
            flags,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null);
        if (saveAsMethod is not null)
        {
            return () => saveAsMethod.Invoke(parserInstance, new object[] { filePath });
        }

        var saveAsWithCompressMethod = parserType.GetMethod(
            "SaveAs",
            flags,
            binder: null,
            types: new[] { typeof(string), typeof(bool) },
            modifiers: null);
        if (saveAsWithCompressMethod is not null)
        {
            return () => saveAsWithCompressMethod.Invoke(parserInstance, new object[] { filePath, false });
        }

        return null;
    }
}
