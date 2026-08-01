using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using ImageMagick.Drawing;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using FormsDialogResult = System.Windows.Forms.DialogResult;
using WpfClipboard = System.Windows.Clipboard;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class ImageEncodingToolWindowViewModel : ObservableObject
{
    private const int PossibleTooLargeThresholdBytes = 30 * 1024;
    private const int TooLargeThresholdBytes = 60 * 1024;
    private const string JpgOutputFormat = "JPG";
    private const string PngOutputFormat = "PNG";
    private const string WebpOutputFormat = "WEBP";

    private bool _isLoadingImage;
    private uint _outputImageWidth;
    private uint _outputImageHeight;
    private byte[] _outputImageBytes = Array.Empty<byte>();

    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private double _compressionRatio = 60;

    [ObservableProperty] private double _resizeScale = 100;

    [ObservableProperty] private bool _isRoundedCornersEnabled = false;

    [ObservableProperty] private double _cornerRadius = 14;

    [ObservableProperty] private IReadOnlyList<string> _outputFormats = new[] { WebpOutputFormat, JpgOutputFormat, PngOutputFormat };

    [ObservableProperty] private string _outputFormat = WebpOutputFormat;

    [ObservableProperty] private bool _hasImage = false;

    [ObservableProperty] private string _filePath = "";

    [ObservableProperty] private string _fileName = "";

    [ObservableProperty] private string _imageSizeText = "-";

    [ObservableProperty] private string _diskSizeText = "-";

    [ObservableProperty] private string _outputSizeLabel = "输出WEBP: ";

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
        var outputFormat = NormalizeOutputFormat(value);
        OutputSizeLabel = $"输出{outputFormat}: ";
        RegenerateOutputIfReady();
    }

    partial void OnIsLuaBase64AutoWrapChanged(bool value)
    {
        RefreshLuaText();
    }

    partial void OnLuaBase64LineLengthChanged(double value)
    {
        RefreshLuaText();
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
            Title = "选择图片",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp;*.ico|所有文件|*.*",
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
                MessageBox.Show("图片文件不存在: " + filePath);
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
                MessageBox.Show("加载图片失败: " + ex.Message);
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
            Title = "保存输出图片",
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
            MessageBox.Show("保存图片失败: " + ex.Message);
        }
    }

    private bool GenerateOutput(string filePath, bool showError)
    {
        try
        {
            using var image = new MagickImage(filePath);
            image.AutoOrient();
            ApplyResize(image, ResizeScale);

            if (IsRoundedCornersEnabled)
            {
                ApplyRoundedCorners(image, CornerRadius);
            }

            var outputFormat = NormalizeOutputFormat(OutputFormat);
            var outputMagickFormat = ResolveMagickFormat(outputFormat);
            if (outputFormat == JpgOutputFormat)
            {
                image.BackgroundColor = MagickColors.White;
                image.Alpha(AlphaOption.Remove);
            }

            ApplyCompression(image, outputFormat, CompressionRatio);

            var outputBytes = image.ToByteArray(outputMagickFormat);
            var base64 = Convert.ToBase64String(outputBytes);
            var base64Bytes = Encoding.ASCII.GetByteCount(base64);

            OutputFormat = outputFormat;
            _outputImageBytes = outputBytes;
            _outputImageWidth = image.Width;
            _outputImageHeight = image.Height;
            ImageSizeText = $"{image.Width} x {image.Height}";
            OutputImageSizeText = FormatBytes(outputBytes.LongLength);
            Base64SizeText = FormatBytes(base64Bytes);
            GameMemorySizeText = FormatGameMemorySize(image.Width, image.Height);
            Base64Text = base64;
            RefreshLuaText();
            OutputPreviewImage = BuildOutputPreviewImage(outputBytes, outputFormat);
            UpdateBase64Warning(base64Bytes);
            return true;
        }
        catch (Exception ex)
        {
            if (showError)
            {
                MessageBox.Show("生成输出图片失败: " + ex.Message);
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
            WpfClipboard.SetDataObject(text, true);
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
        OutputSizeLabel = "输出WEBP: ";
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
        OutputFormat = WebpOutputFormat;
        _outputImageWidth = 0;
        _outputImageHeight = 0;
        _outputImageBytes = Array.Empty<byte>();
    }

    private void RefreshLuaText()
    {
        if (_outputImageWidth == 0 || _outputImageHeight == 0 || string.IsNullOrEmpty(Base64Text))
        {
            return;
        }

        LuaText = BuildLuaText(
            _outputImageWidth,
            _outputImageHeight,
            Base64Text,
            IsLuaBase64AutoWrap,
            ResolveLuaBase64LineLength());
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

    private static string BuildLuaText(uint width, uint height, string base64, bool shouldWrapBase64, int lineLength)
    {
        var luaBase64 = shouldWrapBase64 ? WrapBase64(base64, lineLength) : base64;
        return "local image = {" + Environment.NewLine +
               $"    size = {{{width}, {height}}}," + Environment.NewLine +
               "    base64 = [[" + Environment.NewLine +
               luaBase64 + Environment.NewLine +
               "]]" + Environment.NewLine +
               "}";
    }

    private int ResolveLuaBase64LineLength()
    {
        return Math.Clamp((int)Math.Round(LuaBase64LineLength), 1, 4096);
    }

    private static string WrapBase64(string base64, int lineLength)
    {
        lineLength = Math.Max(1, lineLength);
        var builder = new StringBuilder(base64.Length + base64.Length / lineLength * Environment.NewLine.Length);
        for (var index = 0; index < base64.Length; index += lineLength)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            var length = Math.Min(lineLength, base64.Length - index);
            builder.Append(base64, index, length);
        }

        return builder.ToString();
    }

    private static void ApplyCompression(MagickImage image, string outputFormat, double compressionRatio)
    {
        var normalizedRatio = Math.Clamp((int)Math.Round(compressionRatio), 0, 95);
        if (outputFormat == JpgOutputFormat)
        {
            image.Quality = ResolveJpgQuality(normalizedRatio);
            if (normalizedRatio > 0)
            {
                image.Strip();
            }

            return;
        }

        if (outputFormat == WebpOutputFormat)
        {
            image.Quality = ResolveWebpQuality(normalizedRatio);
            if (normalizedRatio > 0)
            {
                image.Strip();
            }

            return;
        }

        image.Settings.SetDefine(MagickFormat.Png, "compression-level", ResolvePngCompressionLevel(normalizedRatio).ToString());
        if (normalizedRatio > 0)
        {
            image.Strip();
            image.Settings.SetDefine(MagickFormat.Png, "compression-filter", "5");
        }
    }

    private static void ApplyResize(MagickImage image, double resizeScale)
    {
        var normalizedScale = ResolveResizeScale(resizeScale);
        if (normalizedScale >= 100)
        {
            return;
        }

        var targetWidth = Math.Max(1U, (uint)Math.Round(image.Width * normalizedScale / 100.0));
        var targetHeight = Math.Max(1U, (uint)Math.Round(image.Height * normalizedScale / 100.0));
        if (targetWidth == image.Width && targetHeight == image.Height)
        {
            return;
        }

        image.Resize(targetWidth, targetHeight);
    }

    internal static void ApplyRoundedCorners(MagickImage image, double cornerRadius)
    {
        var radius = ResolveCornerRadius(image.Width, image.Height, cornerRadius);
        if (radius <= 0)
        {
            return;
        }

        image.Alpha(AlphaOption.On);

        using var roundedMask = new MagickImage(MagickColors.Transparent, image.Width, image.Height);
        new Drawables()
            .FillColor(MagickColors.White)
            .RoundRectangle(0, 0, image.Width - 1, image.Height - 1, radius, radius)
            .Draw(roundedMask);

        image.Composite(roundedMask, CompositeOperator.DstIn);
    }

    private static uint ResolveJpgQuality(int compressionRatio)
    {
        return (uint)Math.Clamp(100 - compressionRatio, 1, 100);
    }

    private static int ResolvePngCompressionLevel(int compressionRatio)
    {
        if (compressionRatio <= 0)
        {
            return 0;
        }

        return Math.Clamp((int)Math.Ceiling(compressionRatio / 95.0 * 9), 1, 9);
    }

    private static uint ResolveWebpQuality(int compressionRatio)
    {
        return (uint)Math.Clamp(100 - compressionRatio, 1, 100);
    }

    private static int ResolveResizeScale(double resizeScale)
    {
        return Math.Clamp((int)Math.Round(resizeScale), 1, 100);
    }

    internal static int ResolveCornerRadius(uint width, uint height, double cornerRadius)
    {
        if (!double.IsFinite(cornerRadius))
        {
            return 0;
        }

        var maximumRadius = (int)Math.Min(Math.Min(width, height) / 2, int.MaxValue);
        return Math.Clamp((int)Math.Round(cornerRadius), 0, maximumRadius);
    }

    private static string ResolveDefaultOutputFormat(string filePath)
    {
        return WebpOutputFormat;
    }

    private static string NormalizeOutputFormat(string outputFormat)
    {
        if (string.Equals(outputFormat, PngOutputFormat, StringComparison.OrdinalIgnoreCase))
        {
            return PngOutputFormat;
        }

        return string.Equals(outputFormat, WebpOutputFormat, StringComparison.OrdinalIgnoreCase)
            ? WebpOutputFormat
            : JpgOutputFormat;
    }

    private static MagickFormat ResolveMagickFormat(string outputFormat)
    {
        return outputFormat switch
        {
            PngOutputFormat => MagickFormat.Png,
            WebpOutputFormat => MagickFormat.WebP,
            _ => MagickFormat.Jpg
        };
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

    private static string BuildSaveDialogFilter(string outputFormat)
    {
        return outputFormat switch
        {
            PngOutputFormat => "PNG图片|*.png|所有文件|*.*",
            WebpOutputFormat => "WEBP图片|*.webp|所有文件|*.*",
            _ => "JPG图片|*.jpg;*.jpeg|所有文件|*.*"
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

    private void UpdateBase64Warning(int base64Bytes)
    {
        if (base64Bytes > TooLargeThresholdBytes)
        {
            HasBase64Warning = true;
            Base64WarningText = "图片过大, 请考虑拆分lua文件存储; 单个lua文件仅支持64k";
            Base64WarningBrush = Brushes.Crimson;
            return;
        }

        if (base64Bytes > PossibleTooLargeThresholdBytes)
        {
            HasBase64Warning = true;
            Base64WarningText = "图片可能过大";
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
