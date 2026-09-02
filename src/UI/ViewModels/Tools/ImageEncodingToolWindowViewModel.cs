using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using UI.Services;
using UI.Services.ImageEncoding;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using FormsDialogResult = System.Windows.Forms.DialogResult;
using FormsMessageBox = System.Windows.Forms.MessageBox;

namespace UI.ViewModels.Tools;

public partial class ImageEncodingToolWindowViewModel : ObservableObject
{
    private const int PossibleTooLargeThresholdBytes = 30 * 1024;
    private const int TooLargeThresholdBytes = 60 * 1024;
    private const string JpgOutputFormat = ImageEncodingFormats.Jpg;
    private const string PngOutputFormat = ImageEncodingFormats.Png;
    private const string WebpOutputFormat = ImageEncodingFormats.Webp;

    private readonly IImageEncodingService _imageEncodingService;
    private readonly ILocalizationService _localization;
    private bool _isLoadingImage;
    private byte[] _outputImageBytes = Array.Empty<byte>();
    private long _lastBase64SizeBytes;

    /// <summary>
    /// 创建图片编码工具视图模型。
    /// </summary>
    public ImageEncodingToolWindowViewModel(
        IImageEncodingService imageEncodingService,
        ILocalizationService localization)
    {
        _imageEncodingService = imageEncodingService;
        _localization = localization;
        RefreshLocalizedChrome();
        RefreshOutputSizeLabel();
        _localization.LanguageChanged += (_, _) =>
        {
            RefreshLocalizedChrome();
            RefreshOutputSizeLabel();
            if (HasBase64Warning)
            {
                UpdateBase64Warning(_lastBase64SizeBytes);
            }
        };
    }

    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private string _windowTitle = "";

    [ObservableProperty] private string _outputPreviewLabel = "";

    [ObservableProperty] private double _compressionRatio = 60;

    [ObservableProperty] private double _resizeScale = 100;

    [ObservableProperty] private bool _isRoundedCornersEnabled = false;

    [ObservableProperty] private double _cornerRadius = 14;

    [ObservableProperty] private IReadOnlyList<string> _outputFormats = ImageEncodingFormats.All;

    [ObservableProperty] private string _outputFormat = WebpOutputFormat;

    [ObservableProperty] private bool _hasImage = false;

    [ObservableProperty] private string _filePath = "";

    [ObservableProperty] private string _fileName = "";

    [ObservableProperty] private string _imageSizeText = "-";

    [ObservableProperty] private string _diskSizeText = "-";

    [ObservableProperty] private string _outputSizeLabel = "";

    [ObservableProperty] private string _outputImageSizeText = "-";

    [ObservableProperty] private string _base64SizeText = "-";

    [ObservableProperty] private string _gameMemorySizeText = "-";

    [ObservableProperty] private string _base64Text = "";

    [ObservableProperty] private string _luaText = "";

    [ObservableProperty] private bool _isLuaBase64AutoWrap = false;

    [ObservableProperty] private double _luaBase64LineLength = 128;

    [ObservableProperty] private ImageSource _originalPreviewImage = new BitmapImage();

    [ObservableProperty] private ImageSource _outputPreviewImage = new BitmapImage();

    [ObservableProperty] private bool _hasBase64Warning = false;

    [ObservableProperty] private string _base64WarningText = "";

    [ObservableProperty] private Brush _base64WarningBrush = Brushes.Goldenrod;

    partial void OnCompressionRatioChanged(double value)
    {
        RegenerateOutputIfReady();
    }

    partial void OnResizeScaleChanged(double value)
    {
        RegenerateOutputIfReady();
    }

    partial void OnIsRoundedCornersEnabledChanged(bool value)
    {
        RegenerateOutputIfReady();
    }

    partial void OnCornerRadiusChanged(double value)
    {
        RegenerateOutputIfReady();
    }

    partial void OnOutputFormatChanged(string value)
    {
        RefreshOutputSizeLabel();
        RefreshOutputPreviewLabel();
        RegenerateOutputIfReady();
    }

    partial void OnIsLuaBase64AutoWrapChanged(bool value)
    {
        RegenerateOutputIfReady();
    }

    partial void OnLuaBase64LineLengthChanged(double value)
    {
        RegenerateOutputIfReady();
    }

    private void RegenerateOutputIfReady()
    {
        if (_isLoadingImage)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
        {
            GenerateOutput(FilePath, showError: false);
        }
    }

    [RelayCommand]
    private void OpenImage()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = _localization.GetString("ImageEnc_SelectImage"),
            Filter = _localization.GetString("ImageEnc_FilterImages"),
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() != FormsDialogResult.OK)
        {
            return;
        }

        LoadImageFromFile(openFileDialog.FileName);
    }

    public bool LoadImageFromFile(string filePath, bool showError = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        if (!File.Exists(filePath))
        {
            if (showError)
            {
                FormsMessageBox.Show(string.Format(_localization.GetString("ImageEnc_FileMissing"), filePath));
            }

            return false;
        }

        try
        {
            _isLoadingImage = true;
            using var originalImage = new MagickImage(filePath);
            originalImage.AutoOrient();

            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            ImageSizeText = $"{originalImage.Width} x {originalImage.Height}";
            GameMemorySizeText = FormatGameMemorySize(originalImage.Width, originalImage.Height);
            DiskSizeText = FormatBytes(new FileInfo(filePath).Length);
            OriginalPreviewImage = BuildPreviewImage(originalImage.ToByteArray(MagickFormat.Png));
            OutputFormat = ResolveDefaultOutputFormat(filePath);
            HasImage = true;
        }
        catch (Exception ex)
        {
            if (showError)
            {
                FormsMessageBox.Show(string.Format(_localization.GetString("ImageEnc_LoadFailed"), ex.Message));
            }

            ClearImageResult();
            return false;
        }
        finally
        {
            _isLoadingImage = false;
        }

        return GenerateOutput(filePath, showError);
    }

    [RelayCommand]
    private void SaveOutputImage()
    {
        if (_outputImageBytes.Length == 0 || !HasImage)
        {
            return;
        }

        var outputFormat = NormalizeOutputFormat(OutputFormat);
        var extension = ResolveOutputExtension(outputFormat);
        var saveFileDialog = new SaveFileDialog
        {
            Title = _localization.GetString("ImageEnc_SaveOutput"),
            Filter = BuildSaveDialogFilter(outputFormat),
            FileName = BuildDefaultOutputFileName(FileName, extension)
        };

        if (saveFileDialog.ShowDialog() != FormsDialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllBytes(saveFileDialog.FileName, _outputImageBytes);
        }
        catch (Exception ex)
        {
            FormsMessageBox.Show(string.Format(_localization.GetString("ImageEnc_SaveFailed"), ex.Message));
        }
    }

    private bool GenerateOutput(string filePath, bool showError)
    {
        try
        {
            var result = _imageEncodingService.Encode(
                File.ReadAllBytes(filePath),
                new ImageEncodingOptions
                {
                    OutputFormat = OutputFormat,
                    CompressionRatio = CompressionRatio,
                    ResizeScale = ResizeScale,
                    RoundedCorners = IsRoundedCornersEnabled,
                    CornerRadius = CornerRadius,
                    LuaBase64AutoWrap = IsLuaBase64AutoWrap,
                    LuaBase64LineLength = ResolveLuaBase64LineLength()
                });

            var outputBytes = Convert.FromBase64String(result.Base64);
            OutputFormat = result.OutputFormat;
            _outputImageBytes = outputBytes;
            ImageSizeText = $"{result.OutputWidth} x {result.OutputHeight}";
            OutputImageSizeText = FormatBytes(result.OutputSizeBytes);
            Base64SizeText = FormatBytes(result.Base64SizeBytes);
            GameMemorySizeText = FormatBytes(result.GameMemorySizeBytes);
            Base64Text = result.Base64;
            LuaText = result.Lua;
            OutputPreviewImage = BuildOutputPreviewImage(outputBytes, result.OutputFormat);
            UpdateBase64Warning(result.Base64SizeBytes);
            return true;
        }
        catch (Exception ex)
        {
            if (showError)
            {
                FormsMessageBox.Show(string.Format(_localization.GetString("ImageEnc_GenerateFailed"), ex.Message));
            }

            return false;
        }
    }

    [RelayCommand]
    private void Copy(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        try
        {
            Clipboard.SetDataObject(text, true);
        }
        catch
        {
            // 剪贴板被其他程序占用时忽略，避免影响工具窗口。
        }
    }

    [RelayCommand]
    private void Closed()
    {
        ClearImageResult();
    }

    private void ClearImageResult()
    {
        HasImage = false;
        FilePath = "";
        FileName = "";
        ImageSizeText = "-";
        DiskSizeText = "-";
        OutputFormat = WebpOutputFormat;
        RefreshOutputSizeLabel();
        RefreshOutputPreviewLabel();
        OutputImageSizeText = "-";
        Base64SizeText = "-";
        GameMemorySizeText = "-";
        ResizeScale = 100;
        IsRoundedCornersEnabled = false;
        CornerRadius = 14;
        Base64Text = "";
        LuaText = "";
        OriginalPreviewImage = new BitmapImage();
        OutputPreviewImage = new BitmapImage();
        HasBase64Warning = false;
        Base64WarningText = "";
        _lastBase64SizeBytes = 0;
        _outputImageBytes = Array.Empty<byte>();
    }

    private void RefreshLocalizedChrome()
    {
        WindowTitle = _localization.GetString("ImageEnc_Title");
        RefreshOutputPreviewLabel();
    }

    private void RefreshOutputSizeLabel()
    {
        var outputFormat = NormalizeOutputFormat(OutputFormat);
        OutputSizeLabel = string.Format(_localization.GetString("ImageEnc_OutputLabel"), outputFormat);
    }

    private void RefreshOutputPreviewLabel()
    {
        var outputFormat = NormalizeOutputFormat(OutputFormat);
        OutputPreviewLabel = string.Format(_localization.GetString("ImageEnc_OutputFormat"), outputFormat);
    }

    private static BitmapImage BuildPreviewImage(byte[] imageBytes)
    {
        var bitmapImage = new BitmapImage();
        using var stream = new MemoryStream(imageBytes);
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    private static BitmapImage BuildOutputPreviewImage(byte[] imageBytes, string outputFormat)
    {
        if (outputFormat != WebpOutputFormat)
        {
            return BuildPreviewImage(imageBytes);
        }

        using var previewImage = new MagickImage(imageBytes);
        return BuildPreviewImage(previewImage.ToByteArray(MagickFormat.Png));
    }

    private int ResolveLuaBase64LineLength()
    {
        return Math.Clamp((int)Math.Round(LuaBase64LineLength), 1, 4096);
    }

    private static string ResolveDefaultOutputFormat(string filePath)
    {
        return WebpOutputFormat;
    }

    private static string NormalizeOutputFormat(string outputFormat)
    {
        return ImageEncodingFormats.Normalize(outputFormat);
    }

    private static string ResolveOutputExtension(string outputFormat)
    {
        return outputFormat switch
        {
            PngOutputFormat => ".png",
            WebpOutputFormat => ".webp",
            _ => ".jpg"
        };
    }

    private string BuildSaveDialogFilter(string outputFormat)
    {
        return outputFormat switch
        {
            PngOutputFormat => _localization.GetString("ImageEnc_FilterPng"),
            WebpOutputFormat => _localization.GetString("ImageEnc_FilterWebp"),
            _ => _localization.GetString("ImageEnc_FilterJpg")
        };
    }

    private static string BuildDefaultOutputFileName(string fileName, string extension)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrWhiteSpace(nameWithoutExtension))
        {
            nameWithoutExtension = "image";
        }

        return nameWithoutExtension + "_encoded" + extension;
    }

    private static string FormatGameMemorySize(uint width, uint height)
    {
        var bytes = (ulong)width * height * 4UL;
        return bytes > long.MaxValue
            ? FormatBytes((double)bytes)
            : FormatBytes((long)bytes);
    }

    private void UpdateBase64Warning(long base64Bytes)
    {
        _lastBase64SizeBytes = base64Bytes;

        if (base64Bytes > TooLargeThresholdBytes)
        {
            HasBase64Warning = true;
            Base64WarningText = _localization.GetString("ImageEnc_WarnTooLarge");
            Base64WarningBrush = Brushes.Crimson;
            return;
        }

        if (base64Bytes > PossibleTooLargeThresholdBytes)
        {
            HasBase64Warning = true;
            Base64WarningText = _localization.GetString("ImageEnc_WarnMaybeLarge");
            Base64WarningBrush = Brushes.DarkGoldenrod;
            return;
        }

        HasBase64Warning = false;
        Base64WarningText = "";
        Base64WarningBrush = Brushes.Goldenrod;
    }

    private static string FormatBytes(long bytes)
    {
        return FormatBytes((double)bytes, bytes.ToString());
    }

    private static string FormatBytes(ulong bytes)
    {
        return FormatBytes(bytes, bytes.ToString());
    }

    private static string FormatBytes(double bytes)
    {
        return FormatBytes(bytes, Math.Round(bytes).ToString("0"));
    }

    private static string FormatBytes(double bytes, string bytesText)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        var size = (double)bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{bytesText} {units[unitIndex]}"
            : $"{size:0.##} {units[unitIndex]}";
    }
}
