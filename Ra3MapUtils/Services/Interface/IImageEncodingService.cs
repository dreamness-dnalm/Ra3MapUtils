using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

/// <summary>
/// 提供图片尺寸预览和完整编码能力。
/// </summary>
public interface IImageEncodingService
{
    /// <summary>计算编码后的尺寸和占用信息。</summary>
    ImageEncodingPreviewResult Preview(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options);

    /// <summary>生成完整 Base64 和 Lua 编码结果。</summary>
    ImageEncodingResult Encode(ReadOnlyMemory<byte> sourceImage, ImageEncodingOptions options);
}
