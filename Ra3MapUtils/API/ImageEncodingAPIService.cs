using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.API;

/// <summary>
/// 图片编码接口。
/// </summary>
[ApiController]
[Route("api/image-encoding")]
public sealed class ImageEncodingAPIService : ControllerBase
{
    private readonly IImageEncodingService _imageEncodingService;

    /// <summary>
    /// 创建图片编码控制器。
    /// </summary>
    public ImageEncodingAPIService(IImageEncodingService imageEncodingService)
    {
        _imageEncodingService = imageEncodingService;
    }

    /// <summary>
    /// 预览图片编码后的尺寸与字节占用，不返回 Base64 正文。
    /// </summary>
    /// <param name="request">图片文件和编码参数。</param>
    /// <returns>原始/输出尺寸、输出大小、Base64 大小和游戏内存估算。</returns>
    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImageEncodingPreviewResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<ImageEncodingPreviewResult>> Preview([FromForm] ImageEncodingFormRequest request)
    {
        if (request?.File == null || request.File.Length == 0)
        {
            return ApiResponse<ImageEncodingPreviewResult>.IllegalArgument("图片文件不能为空。");
        }

        try
        {
            var sourceImage = await ReadImageAsync(request.File);
            var result = _imageEncodingService.Preview(sourceImage, request.ToOptions());
            return ApiResponse<ImageEncodingPreviewResult>.Success(result);
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<ImageEncodingPreviewResult>.IllegalArgument(ex.Message);
        }
        catch (Exception ex)
        {
            return ApiResponse<ImageEncodingPreviewResult>.UnknownError(ex.Message);
        }
    }

    /// <summary>
    /// 获取图片的完整编码结果。
    /// </summary>
    /// <param name="request">图片文件和编码参数。</param>
    /// <returns>编码元数据、Base64 正文和 Lua 片段。</returns>
    [HttpPost("encode")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImageEncodingResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<ImageEncodingResult>> Encode([FromForm] ImageEncodingFormRequest request)
    {
        if (request?.File == null || request.File.Length == 0)
        {
            return ApiResponse<ImageEncodingResult>.IllegalArgument("图片文件不能为空。");
        }

        try
        {
            var sourceImage = await ReadImageAsync(request.File);
            var result = _imageEncodingService.Encode(sourceImage, request.ToOptions());
            return ApiResponse<ImageEncodingResult>.Success(result);
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<ImageEncodingResult>.IllegalArgument(ex.Message);
        }
        catch (Exception ex)
        {
            return ApiResponse<ImageEncodingResult>.UnknownError(ex.Message);
        }
    }

    private static async Task<byte[]> ReadImageAsync(IFormFile file)
    {
        if (file.Length > int.MaxValue)
        {
            throw new ArgumentException("图片文件过大。");
        }

        using var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream);
        return stream.ToArray();
    }
}

/// <summary>
/// 图片编码的 multipart/form-data 请求参数。
/// </summary>
public sealed class ImageEncodingFormRequest
{
    /// <summary>
    /// 待编码的图片文件。
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// 输出格式：WEBP、JPG 或 PNG。
    /// </summary>
    public string OutputFormat { get; set; } = ImageEncodingFormats.Webp;

    /// <summary>
    /// 压缩比率，范围 0 到 95。
    /// </summary>
    public double CompressionRatio { get; set; } = 60;

    /// <summary>
    /// 输出尺寸百分比，范围 1 到 100。
    /// </summary>
    public double ResizeScale { get; set; } = 100;

    /// <summary>
    /// 是否为图片四角添加圆角。
    /// </summary>
    public bool RoundedCorners { get; set; }

    /// <summary>
    /// 圆角半径（像素），范围 0 到 4096；实际值不会超过图片短边的一半。
    /// </summary>
    public double CornerRadius { get; set; } = 14;

    /// <summary>
    /// Lua 片段中的 Base64 是否自动换行。
    /// </summary>
    public bool LuaBase64AutoWrap { get; set; }

    /// <summary>
    /// Lua Base64 每行字符数，范围 1 到 4096。
    /// </summary>
    public int LuaBase64LineLength { get; set; } = 128;

    internal ImageEncodingOptions ToOptions()
    {
        return new ImageEncodingOptions
        {
            OutputFormat = OutputFormat,
            CompressionRatio = CompressionRatio,
            ResizeScale = ResizeScale,
            RoundedCorners = RoundedCorners,
            CornerRadius = CornerRadius,
            LuaBase64AutoWrap = LuaBase64AutoWrap,
            LuaBase64LineLength = LuaBase64LineLength
        };
    }
}
