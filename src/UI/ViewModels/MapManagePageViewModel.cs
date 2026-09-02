using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Maps;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using UI.Dialogs;
using UI.Services;
using UI.Views.Tools;

namespace UI.ViewModels;

public partial class MapManagePageViewModel : ObservableObject
{
    private readonly IMapCatalogService _mapCatalogService;
    private readonly IMapFileService _mapFileService;
    private readonly ILocalizationService _localization;
    private readonly IServiceProvider _services;
    private List<MapEntry> _allMaps = new();
    private bool _suppressSelectionSync;

    public MapManagePageViewModel(
        IMapCatalogService mapCatalogService,
        IMapFileService mapFileService,
        ILocalizationService localization,
        IServiceProvider services)
    {
        _mapCatalogService = mapCatalogService;
        _mapFileService = mapFileService;
        _localization = localization;
        _services = services;
        SelectedMaps.CollectionChanged += OnSelectedMapsChanged;
        PreviewPlaceholder = L("Maps_PreviewPlaceholder", "选择左侧地图以查看预览");
        _localization.LanguageChanged += (_, _) =>
        {
            if (!HasError)
            {
                StatusMessage = _allMaps.Count == 0
                    ? L("Maps_NoneFound", "未找到地图。")
                    : string.Format(L("Maps_CountFormat", "共 {0} 张地图。"), _allMaps.Count);
            }

            UpdatePreviewFromSelection();
        };
        Refresh();
    }

    private string L(string key, string fallback) => _localization.GetString(key, fallback);

    public ObservableCollection<MapEntry> Maps { get; } = new();

    public ObservableCollection<MapEntry> SelectedMaps { get; } = new();

    public ObservableCollection<string> PreviewFiles { get; } = new();

    [ObservableProperty]
    private string _mapsRoot = "";

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private MapEntry? _selectedMap;

    [ObservableProperty]
    private MapPreview? _preview;

    [ObservableProperty]
    private BitmapImage? _thumbnail;

    [ObservableProperty]
    private bool _hasPreview;

    [ObservableProperty]
    private bool _isMultiSelectSummary;

    [ObservableProperty]
    private string _previewPlaceholder = "选择左侧地图以查看预览";

    [ObservableProperty]
    private string _multiSelectSummary = "";

    [ObservableProperty]
    private bool _isHintVisible = true;

    public bool HasSingleSelection => SelectedMaps.Count == 1;

    public bool HasAnySelection => SelectedMaps.Count > 0;

    public bool CanDeleteSelection => SelectedMaps.Count > 0;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void OnSelectedMapsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasSingleSelection));
        OnPropertyChanged(nameof(HasAnySelection));
        OnPropertyChanged(nameof(CanDeleteSelection));
        OpenFolderCommand.NotifyCanExecuteChanged();
        RenameCommand.NotifyCanExecuteChanged();
        CloneCommand.NotifyCanExecuteChanged();
        CompressCommand.NotifyCanExecuteChanged();
        EditDisplayNameCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        OpenLuaImportCommand.NotifyCanExecuteChanged();
        UpdatePreviewFromSelection();
    }

    public void SyncSelectionFromList(IList<object> selectedItems)
    {
        if (_suppressSelectionSync)
        {
            return;
        }

        _suppressSelectionSync = true;
        try
        {
            SelectedMaps.Clear();
            foreach (var item in selectedItems.OfType<MapEntry>())
            {
                SelectedMaps.Add(item);
            }

            SelectedMap = SelectedMaps.Count == 1 ? SelectedMaps[0] : null;
        }
        finally
        {
            _suppressSelectionSync = false;
        }
    }

    [RelayCommand]
    private void DismissHint() => IsHintVisible = false;

    [RelayCommand]
    private void Refresh()
    {
        var retained = SelectedMaps.Select(m => m.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = _mapCatalogService.ListMaps();
        MapsRoot = result.MapsRoot;
        HasError = !result.Succeeded;
        StatusMessage = result.Succeeded
            ? (result.Maps.Count == 0
                ? L("Maps_NoneFound", "未找到地图。")
                : string.Format(L("Maps_CountFormat", "共 {0} 张地图。"), result.Maps.Count))
            : result.ErrorMessage;

        _allMaps = result.Maps.ToList();
        ApplyFilter(retained);
    }

    [RelayCommand]
    private void OpenMapsRoot()
    {
        var result = _mapFileService.OpenInExplorer(MapsRoot);
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_OpenFolderFailed", "打开目录失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void OpenFolder()
    {
        var map = SelectedMaps[0];
        var result = _mapFileService.OpenInExplorer(map.DirectoryPath);
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_OpenFailed", "打开失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void Rename()
    {
        var map = SelectedMaps[0];
        if (!TextInputDialog.TryPrompt(
                GetOwner(),
                L("Maps_Rename", "重命名"),
                L("Maps_RenamePrompt", "请输入新地图名"),
                map.Name,
                out var newName))
        {
            return;
        }

        var result = _mapFileService.Rename(map.DirectoryPath, newName.Trim());
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_RenameFailed", "重命名失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        RefreshKeeping(newName.Trim());
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void Clone()
    {
        var map = SelectedMaps[0];
        if (!TextInputDialog.TryPrompt(
                GetOwner(),
                L("Maps_SaveAs", "另存为"),
                L("Maps_SaveAsPrompt", "请输入新地图名"),
                map.Name + "_COPY",
                out var newName))
        {
            return;
        }

        newName = newName.Trim();
        string? newDisplay = null;
        var oldDisplay = _mapFileService.GetDisplayName(map.DirectoryPath);
        if (oldDisplay is not null)
        {
            if (TextInputDialog.TryPrompt(
                    GetOwner(),
                    L("Maps_DisplayName", "游戏内显示名"),
                    L("Maps_DisplayNamePrompt", "请输入游戏显示的地图名"),
                    oldDisplay + "_COPY",
                    out var displayInput))
            {
                newDisplay = displayInput.Trim();
            }
            else
            {
                newDisplay = oldDisplay + "_COPY";
            }
        }

        var result = _mapFileService.Clone(map.DirectoryPath, newName, newDisplay);
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_SaveAsFailed", "另存为失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        RefreshKeeping(newName);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void Compress()
    {
        var map = SelectedMaps[0];
        var confirm = LocalizedMessageBox.ShowRaw(
            string.Format(L("Maps_CompressConfirm", "确认压缩地图「{0}」？"), map.Name),
            L("Maps_Compress", "压缩"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        var result = _mapFileService.CompressZip(map.DirectoryPath);
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_CompressFailed", "压缩失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        LocalizedMessageBox.ShowRaw(
            string.Format(L("Maps_CompressDoneBody", "已生成：{0}"), result.OutputPath),
            L("Maps_CompressDone", "压缩完成"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void EditDisplayName()
    {
        var map = SelectedMaps[0];
        var current = _mapFileService.GetDisplayName(map.DirectoryPath) ?? map.Name;
        if (!TextInputDialog.TryPrompt(
                GetOwner(),
                L("Maps_EditDisplayName", "修改游戏内显示名"),
                L("Maps_EditDisplayNamePrompt", "请输入 map.str 显示名"),
                current,
                out var name))
        {
            return;
        }

        var result = _mapFileService.SetDisplayName(map.DirectoryPath, name.Trim());
        if (!result.Succeeded)
        {
            LocalizedMessageBox.ShowRaw(
                result.ErrorMessage ?? "",
                L("Maps_EditFailed", "修改失败"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        UpdatePreviewFromSelection();
        StatusMessage = L("Maps_DisplayNameUpdated", "已更新游戏内显示名。");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSingleMap))]
    private void OpenLuaImport()
    {
        var map = SelectedMaps[0];
        var window = _services.GetRequiredService<LuaImportManagerWindow>();
        window.Owner = GetOwner();
        window.Initialize(map.Name, map.DirectoryPath);
        window.Show();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteDelete))]
    private void Delete()
    {
        var targets = SelectedMaps.ToList();
        if (targets.Count == 0)
        {
            return;
        }

        var message = targets.Count == 1
            ? string.Format(L("Maps_DeleteConfirmOne", "确认删除地图「{0}」？此操作不可恢复。"), targets[0].Name)
            : string.Format(L("Maps_DeleteConfirmMany", "确认删除所选的 {0} 张地图？此操作不可恢复。"), targets.Count);
        var confirm = LocalizedMessageBox.ShowRaw(
            message,
            L("Maps_DeleteConfirmTitle", "删除确认"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        if (targets.Count == 1)
        {
            var result = _mapFileService.Delete(targets[0].DirectoryPath);
            if (!result.Succeeded)
            {
                LocalizedMessageBox.ShowRaw(
                    result.ErrorMessage ?? "",
                    L("Maps_DeleteFailed", "删除失败"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            Refresh();
            return;
        }

        var batch = _mapFileService.DeleteMany(targets.Select(t => t.DirectoryPath));
        Refresh();
        if (batch.Failures.Count == 0)
        {
            StatusMessage = string.Format(L("Maps_DeletedFormat", "已删除 {0} 张地图。"), batch.SuccessCount);
            return;
        }

        var detail = string.Join("\n", batch.Failures.Take(8));
        if (batch.Failures.Count > 8)
        {
            detail += "\n" + string.Format(L("Maps_BatchDeleteMore", "…另有 {0} 条"), batch.Failures.Count - 8);
        }

        LocalizedMessageBox.ShowRaw(
            string.Format(L("Maps_BatchDeleteBody", "成功 {0}，失败 {1}：\n{2}"), batch.SuccessCount, batch.Failures.Count, detail),
            L("Maps_BatchDeleteTitle", "批量删除结果"),
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private bool CanExecuteSingleMap() => SelectedMaps.Count == 1;

    private bool CanExecuteDelete() => SelectedMaps.Count > 0;

    private void ApplyFilter(HashSet<string>? retainNames = null)
    {
        var keyword = SearchText.Trim();
        IEnumerable<MapEntry> filtered = _allMaps;
        if (!string.IsNullOrEmpty(keyword))
        {
            filtered = _allMaps.Where(m =>
                m.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.ToList();
        Maps.Clear();
        foreach (var map in list)
        {
            Maps.Add(map);
        }

        _suppressSelectionSync = true;
        try
        {
            SelectedMaps.Clear();
            if (retainNames is { Count: > 0 })
            {
                foreach (var map in list.Where(m => retainNames.Contains(m.Name)))
                {
                    SelectedMaps.Add(map);
                }
            }

            SelectedMap = SelectedMaps.Count == 1 ? SelectedMaps[0] : null;
        }
        finally
        {
            _suppressSelectionSync = false;
        }

        UpdatePreviewFromSelection();
        OnPropertyChanged(nameof(HasSingleSelection));
        OnPropertyChanged(nameof(HasAnySelection));
        OnPropertyChanged(nameof(CanDeleteSelection));
        OpenFolderCommand.NotifyCanExecuteChanged();
        RenameCommand.NotifyCanExecuteChanged();
        CloneCommand.NotifyCanExecuteChanged();
        CompressCommand.NotifyCanExecuteChanged();
        EditDisplayNameCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        OpenLuaImportCommand.NotifyCanExecuteChanged();
    }

    private void RefreshKeeping(string mapName)
    {
        Refresh();
        var match = Maps.FirstOrDefault(m =>
            string.Equals(m.Name, mapName, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return;
        }

        _suppressSelectionSync = true;
        try
        {
            SelectedMaps.Clear();
            SelectedMaps.Add(match);
            SelectedMap = match;
        }
        finally
        {
            _suppressSelectionSync = false;
        }

        UpdatePreviewFromSelection();
    }

    private void UpdatePreviewFromSelection()
    {
        PreviewFiles.Clear();
        Thumbnail = null;
        Preview = null;
        HasPreview = false;
        IsMultiSelectSummary = false;

        if (SelectedMaps.Count == 0)
        {
            PreviewPlaceholder = L("Maps_PreviewPlaceholder", "选择左侧地图以查看预览");
            return;
        }

        if (SelectedMaps.Count > 1)
        {
            IsMultiSelectSummary = true;
            HasPreview = true;
            MultiSelectSummary = string.Format(
                L("Maps_MultiSelectSummary", "已选择 {0} 张地图。\n右键可批量删除；其他操作需只选一张。"),
                SelectedMaps.Count);
            return;
        }

        var entry = SelectedMaps[0];
        var preview = _mapCatalogService.GetPreview(entry.DirectoryPath);
        if (preview is null)
        {
            PreviewPlaceholder = L("Maps_PreviewUnavailable", "无法读取该地图的预览信息。");
            return;
        }

        Preview = preview;
        HasPreview = true;
        foreach (var file in preview.Files)
        {
            PreviewFiles.Add(file);
        }

        Thumbnail = TryLoadThumbnail(preview.ThumbnailPath);
    }

    private static BitmapImage? TryLoadThumbnail(string? thumbnailPath)
    {
        if (string.IsNullOrWhiteSpace(thumbnailPath) || !File.Exists(thumbnailPath))
        {
            return null;
        }

        try
        {
            using var magickImage = new MagickImage(thumbnailPath);
            var imageBytes = magickImage.ToByteArray(MagickFormat.Bmp);
            using var ms = new MemoryStream(imageBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private static Window? GetOwner() =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        ?? Application.Current?.MainWindow;
}
