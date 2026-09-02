namespace UI.Services.ImageEncoding;

/// <summary>
/// 图片编码支持的输出格式。
/// </summary>
public static class ImageEncodingFormats
{
    /// <summary>WebP 输出格式。</summary>
    public const string Webp = "WEBP";

    /// <summary>JPEG 输出格式。</summary>
    public const string Jpg = "JPG";

    /// <summary>PNG 输出格式。</summary>
    public const string Png = "PNG";

    /// <summary>所有支持的输出格式。</summary>
    public static IReadOnlyList<string> All { get; } = new[] { Webp, Jpg, Png };

    /// <summary>
    /// 将调用方传入的格式名规范化为支持的格式常量。
    /// </summary>
    public static string Normalize(string? outputFormat)
    {
        if (string.Equals(outputFormat, Webp, StringComparison.OrdinalIgnoreCase))
        {
            return Webp;
        }

        if (string.Equals(outputFormat, Jpg, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(outputFormat, "JPEG", StringComparison.OrdinalIgnoreCase))
        {
            return Jpg;
        }

        if (string.Equals(outputFormat, Png, StringComparison.OrdinalIgnoreCase))
        {
            return Png;
        }

        throw new ArgumentException("输出格式仅支持 WEBP、JPG 或 PNG。", nameof(outputFormat));
    }
}

/// <summary>
/// 图片编码参数。
/// </summary>
public sealed class ImageEncodingOptions
{
    /// <summary>输出格式。</summary>
    public string OutputFormat { get; set; } = ImageEncodingFormats.Webp;

    /// <summary>压缩比率，范围 0 到 95。</summary>
    public double CompressionRatio { get; set; } = 60;

    /// <summary>输出尺寸百分比，范围 1 到 100。</summary>
    public double ResizeScale { get; set; } = 100;

    /// <summary>是否添加圆角。</summary>
    public bool RoundedCorners { get; set; }

    /// <summary>圆角半径（像素）。</summary>
    public double CornerRadius { get; set; } = 14;

    /// <summary>Lua 片段中的 Base64 是否换行。</summary>
    public bool LuaBase64AutoWrap { get; set; }

    /// <summary>Lua Base64 每行字符数。</summary>
    public int LuaBase64LineLength { get; set; } = 128;
}

/// <summary>
/// 图片编码后的尺寸与占用预览。
/// </summary>
public class ImageEncodingPreviewResult
{
    /// <summary>实际输出格式。</summary>
    public string OutputFormat { get; init; } = ImageEncodingFormats.Webp;

    /// <summary>原图宽度。</summary>
    public uint OriginalWidth { get; init; }

    /// <summary>原图高度。</summary>
    public uint OriginalHeight { get; init; }

    /// <summary>输出图片宽度。</summary>
    public uint OutputWidth { get; init; }

    /// <summary>输出图片高度。</summary>
    public uint OutputHeight { get; init; }

    /// <summary>上传内容的字节数。</summary>
    public long SourceSizeBytes { get; init; }

    /// <summary>编码后图片的字节数。</summary>
    public long OutputSizeBytes { get; init; }

    /// <summary>Base64 文本的 ASCII 字节数。</summary>
    public long Base64SizeBytes { get; init; }

    /// <summary>按 RGBA32 估算的游戏内存字节数。</summary>
    public ulong GameMemorySizeBytes { get; init; }
}

/// <summary>
/// 完整图片编码结果。
/// </summary>
public sealed class ImageEncodingResult : ImageEncodingPreviewResult
{
    /// <summary>编码后图片的 Base64 文本。</summary>
    public string Base64 { get; init; } = string.Empty;

    /// <summary>可直接使用的 Lua 图片片段。</summary>
    public string Lua { get; init; } = string.Empty;
}
