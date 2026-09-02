using System.Text;
using ImageMagick;
using ImageMagick.Drawing;

namespace UI.Services.ImageEncoding;

/// <summary>
/// 基于 ImageMagick 的图片编码服务。
/// </summary>
public sealed class ImageEncodingService : IImageEncodingService
{
    /// <inheritdoc />
    public ImageEncodingPreviewResult Preview(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options)
    {
        using var encoded = EncodeCore(sourceImage, options);
        return CreatePreview(encoded, sourceImage.Length);
    }

    /// <inheritdoc />
    public ImageEncodingResult Encode(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options)
    {
        ValidateLuaOptions(options);
        using var encoded = EncodeCore(sourceImage, options);
        var preview = CreatePreview(encoded, sourceImage.Length);
        var base64 = Convert.ToBase64String(encoded.OutputBytes);

        return new ImageEncodingResult
        {
            OutputFormat = preview.OutputFormat,
            OriginalWidth = preview.OriginalWidth,
            OriginalHeight = preview.OriginalHeight,
            OutputWidth = preview.OutputWidth,
            OutputHeight = preview.OutputHeight,
            SourceSizeBytes = preview.SourceSizeBytes,
            OutputSizeBytes = preview.OutputSizeBytes,
            Base64SizeBytes = preview.Base64SizeBytes,
            GameMemorySizeBytes = preview.GameMemorySizeBytes,
            Base64 = base64,
            Lua = BuildLuaText(
                preview.OutputWidth,
                preview.OutputHeight,
                base64,
                options.LuaBase64AutoWrap,
                options.LuaBase64LineLength)
        };
    }

    private static EncodedImage EncodeCore(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options)
    {
        Validate(sourceImage, options);

        var outputFormat = ImageEncodingFormats.Normalize(options.OutputFormat);
        var magickFormat = ResolveMagickFormat(outputFormat);
        MagickImage image;
        try
        {
            image = new MagickImage(sourceImage.ToArray());
        }
        catch (MagickException ex)
        {
            throw new ArgumentException("无法读取上传的图片内容。", nameof(sourceImage), ex);
        }

        try
        {
            image.AutoOrient();
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            ApplyResize(image, options.ResizeScale);
            if (options.RoundedCorners)
            {
                ApplyRoundedCorners(image, options.CornerRadius);
            }

            if (outputFormat == ImageEncodingFormats.Jpg)
            {
                image.BackgroundColor = MagickColors.White;
                image.Alpha(AlphaOption.Remove);
            }

            ApplyCompression(image, outputFormat, options.CompressionRatio);
            var outputBytes = image.ToByteArray(magickFormat);
            return new EncodedImage(image, outputBytes, outputFormat, originalWidth, originalHeight);
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    private static ImageEncodingPreviewResult CreatePreview(EncodedImage encoded, int sourceSizeBytes)
    {
        return new ImageEncodingPreviewResult
        {
            OutputFormat = encoded.OutputFormat,
            OriginalWidth = encoded.OriginalWidth,
            OriginalHeight = encoded.OriginalHeight,
            OutputWidth = encoded.Image.Width,
            OutputHeight = encoded.Image.Height,
            SourceSizeBytes = sourceSizeBytes,
            OutputSizeBytes = encoded.OutputBytes.LongLength,
            Base64SizeBytes = CalculateBase64Size(encoded.OutputBytes.LongLength),
            GameMemorySizeBytes = (ulong)encoded.Image.Width * encoded.Image.Height * 4UL
        };
    }

    private static void Validate(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options)
    {
        if (sourceImage.IsEmpty)
        {
            throw new ArgumentException("图片内容不能为空。", nameof(sourceImage));
        }

        ArgumentNullException.ThrowIfNull(options);
        ImageEncodingFormats.Normalize(options.OutputFormat);

        ValidateRange(options.CompressionRatio, 0, 95, "压缩比率");
        ValidateRange(options.ResizeScale, 1, 100, "尺寸缩放");
        ValidateRange(options.CornerRadius, 0, 4096, "圆角半径");
    }

    private static void ValidateLuaOptions(ImageEncodingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.LuaBase64LineLength is < 1 or > 4096)
        {
            throw new ArgumentException("Lua Base64 每行长度必须在 1 到 4096 之间。", nameof(options));
        }
    }

    private static void ValidateRange(double value, double minimum, double maximum, string fieldName)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum)
        {
            throw new ArgumentException($"{fieldName}必须在 {minimum:0} 到 {maximum:0} 之间。");
        }
    }

    private static void ApplyCompression(MagickImage image, string outputFormat, double compressionRatio)
    {
        var normalizedRatio = (int)Math.Round(compressionRatio);
        if (outputFormat == ImageEncodingFormats.Jpg)
        {
            image.Quality = (uint)Math.Clamp(100 - normalizedRatio, 1, 100);
        }
        else if (outputFormat == ImageEncodingFormats.Webp)
        {
            image.Quality = (uint)Math.Clamp(100 - normalizedRatio, 1, 100);
        }
        else
        {
            var compressionLevel = normalizedRatio <= 0
                ? 0
                : Math.Clamp((int)Math.Ceiling(normalizedRatio / 95.0 * 9), 1, 9);
            image.Settings.SetDefine(MagickFormat.Png, "compression-level", compressionLevel.ToString());
            if (normalizedRatio > 0)
            {
                image.Settings.SetDefine(MagickFormat.Png, "compression-filter", "5");
            }
        }

        if (normalizedRatio > 0)
        {
            image.Strip();
        }
    }

    private static void ApplyResize(MagickImage image, double resizeScale)
    {
        var normalizedScale = (int)Math.Round(resizeScale);
        if (normalizedScale >= 100)
        {
            return;
        }

        var targetWidth = Math.Max(1U, (uint)Math.Round(image.Width * normalizedScale / 100.0));
        var targetHeight = Math.Max(1U, (uint)Math.Round(image.Height * normalizedScale / 100.0));
        if (targetWidth != image.Width || targetHeight != image.Height)
        {
            image.Resize(targetWidth, targetHeight);
        }
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

    internal static int ResolveCornerRadius(uint width, uint height, double cornerRadius)
    {
        if (!double.IsFinite(cornerRadius))
        {
            return 0;
        }

        var maximumRadius = (int)Math.Min(Math.Min(width, height) / 2, int.MaxValue);
        return Math.Clamp((int)Math.Round(cornerRadius), 0, maximumRadius);
    }

    private static MagickFormat ResolveMagickFormat(string outputFormat)
    {
        return outputFormat switch
        {
            ImageEncodingFormats.Png => MagickFormat.Png,
            ImageEncodingFormats.Webp => MagickFormat.WebP,
            _ => MagickFormat.Jpg
        };
    }

    private static long CalculateBase64Size(long byteCount)
    {
        return checked(((byteCount + 2) / 3) * 4);
    }

    private static string BuildLuaText(
        uint width,
        uint height,
        string base64,
        bool shouldWrapBase64,
        int lineLength)
    {
        var luaBase64 = shouldWrapBase64 ? WrapBase64(base64, lineLength) : base64;
        return "local image = {" + Environment.NewLine +
               $"    size = {{{width}, {height}}}," + Environment.NewLine +
               "    base64 = [[" + Environment.NewLine +
               luaBase64 + Environment.NewLine +
               "]]" + Environment.NewLine +
               "}";
    }

    private static string WrapBase64(string base64, int lineLength)
    {
        var builder = new StringBuilder(base64.Length + base64.Length / lineLength * Environment.NewLine.Length);
        for (var index = 0; index < base64.Length; index += lineLength)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(base64, index, Math.Min(lineLength, base64.Length - index));
        }

        return builder.ToString();
    }

    private sealed class EncodedImage : IDisposable
    {
        public EncodedImage(
            MagickImage image,
            byte[] outputBytes,
            string outputFormat,
            uint originalWidth,
            uint originalHeight)
        {
            Image = image;
            OutputBytes = outputBytes;
            OutputFormat = outputFormat;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public MagickImage Image { get; }

        public byte[] OutputBytes { get; }

        public string OutputFormat { get; }

        public uint OriginalWidth { get; }

        public uint OriginalHeight { get; }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}
