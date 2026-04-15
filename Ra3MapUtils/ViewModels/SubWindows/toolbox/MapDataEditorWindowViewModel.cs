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
using Dreamness.RA3.Map.Parser.Core.MapScb;
using Dreamness.Ra3.Map.Parser.Asset.Base;
using Dreamness.Ra3.Map.Parser.Asset.Impl.Player;
using Dreamness.Ra3.Map.Parser.Core.Base;
using Dreamness.Ra3.Map.Parser.Core.Map;
using Ra3MapUtils.Models;
using Ra3MapUtils.Views.SubWindows.toolbox;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class MapDataEditorWindowViewModel : ObservableObject
{
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

    [ObservableProperty] private ObservableCollection<MapDataAssetTreeNode> _assetTree = new();

    [ObservableProperty] private MapDataAssetTreeNode? _selectedAssetNode;

    [ObservableProperty] private MapDataAssetDetailModel _selectedAssetDetail = MapDataAssetDetailModel.Empty();

    private bool _hasSidesListAsset;

    public MapDataEditorWindow? _mapDataEditorWindow;

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

            BuildPlayerTree(context);
            SelectedAssetNode = AssetTree.FirstOrDefault();
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
            MessageBox.Show("Failed to load map data: " + ex.Message);
            return false;
        }
    }

    public void TryActivateWindow()
    {
        if (_mapDataEditorWindow is null || !_mapDataEditorWindow.IsVisible)
        {
            return;
        }

        if (_mapDataEditorWindow.WindowState == WindowState.Minimized)
        {
            _mapDataEditorWindow.WindowState = WindowState.Normal;
        }

        _mapDataEditorWindow.Activate();
    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.MapDataEditorWindowOpened = false;
        _mapDataEditorWindow = null;
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

    private void BuildPlayerTree(BaseContext context)
    {
        AssetTree.Clear();

        var rootNode = new MapDataAssetTreeNode
        {
            DisplayName = "\u73a9\u5bb6\u5217\u8868",
            Kind = MapDataAssetTreeNodeKind.Root,
            IsExpanded = true
        };

        var sidesListAsset = FindSidesListAsset(context);
        _hasSidesListAsset = sidesListAsset is not null;

        if (sidesListAsset is not null)
        {
            foreach (var player in sidesListAsset.PlayerDataList)
            {
                rootNode.Children.Add(new MapDataAssetTreeNode
                {
                    DisplayName = SafeValue(() => player.Name),
                    Kind = MapDataAssetTreeNodeKind.Player,
                    Player = player
                });
            }
        }

        AssetTree.Add(rootNode);
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

    private MapDataAssetDetailModel BuildDetail(MapDataAssetTreeNode? node)
    {
        if (node is null)
        {
            return MapDataAssetDetailModel.Empty();
        }

        return node.Kind switch
        {
            MapDataAssetTreeNodeKind.Root => BuildRootDetail(node),
            MapDataAssetTreeNodeKind.Player => BuildPlayerDetail(node),
            _ => MapDataAssetDetailModel.Empty()
        };
    }

    private MapDataAssetDetailModel BuildRootDetail(MapDataAssetTreeNode node)
    {
        var detail = new MapDataAssetDetailModel
        {
            Title = node.DisplayName,
            Description = _hasSidesListAsset
                ? "SidesListAsset found."
                : "SidesListAsset not found. Player list is empty."
        };

        AddField(detail, "NodeType", "Root");
        AddField(detail, "PlayerCount", node.Children.Count.ToString());
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
        AddField(detail, "IsHuman", SafeValue(() => ToBoolText(player.IsHuman)));
        AddField(detail, "Faction", SafeValue(() => player.Faction));
        AddField(detail, "AllyPlayerNames", SafeValue(() => ToListText(player.AllyPlayerNames)));
        AddField(detail, "EnemyPlayerNames", SafeValue(() => ToListText(player.EnemyPlayerNames)));

        AddField(detail, "Personality", SafeValue(() => ToText(player.Personality)));
        AddField(detail, "FactionIcon", SafeValue(() => ToText(player.FactionIcon)));

        AddField(detail, "BaseBuilder", SafeValue(() => ToDifficultyText(player.BaseBuilder)));
        AddField(detail, "UnitBuilder", SafeValue(() => ToDifficultyText(player.UnitBuilder)));
        AddField(detail, "TeamBuilder", SafeValue(() => ToDifficultyText(player.TeamBuilder)));
        AddField(detail, "EconomyBuilder", SafeValue(() => ToDifficultyText(player.EconomyBuilder)));
        AddField(detail, "WallBuilder", SafeValue(() => ToDifficultyText(player.WallBuilder)));
        AddField(detail, "UnitUpgrader", SafeValue(() => ToDifficultyText(player.UnitUpgrader)));
        AddField(detail, "ScienceUpgrader", SafeValue(() => ToDifficultyText(player.ScienceUpgrader)));
        AddField(detail, "Tactical", SafeValue(() => ToDifficultyText(player.Tactical)));
        AddField(detail, "OpeningMover", SafeValue(() => ToDifficultyText(player.OpeningMover)));

        AddField(detail, "Color", SafeValue(() => ToText(player.Color)));
        AddField(detail, "RadarColor", SafeValue(() => ToText(player.RadarColor)));

        return detail;
    }

    private static void AddField(MapDataAssetDetailModel detail, string label, string value)
    {
        detail.Fields.Add(new MapDataAssetDetailFieldItem
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

