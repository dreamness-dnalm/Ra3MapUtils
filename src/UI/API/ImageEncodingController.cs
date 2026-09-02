using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UI.Services.ImageEncoding;

namespace UI.API;

[ApiController]
[Route("api/image-encoding")]
public sealed class ImageEncodingController : ControllerBase
{
    private readonly IImageEncodingService _imageEncodingService;

    public ImageEncodingController(IImageEncodingService imageEncodingService)
    {
        _imageEncodingService = imageEncodingService;
    }

    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImageEncodingPreviewResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<ImageEncodingPreviewResult>> Preview([FromForm] ImageEncodingFormRequest request)
    {
        if (request?.File is null || request.File.Length == 0)
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

    [HttpPost("encode")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImageEncodingResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<ImageEncodingResult>> Encode([FromForm] ImageEncodingFormRequest request)
    {
        if (request?.File is null || request.File.Length == 0)
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

public sealed class ImageEncodingFormRequest
{
    public IFormFile? File { get; set; }

    public string OutputFormat { get; set; } = ImageEncodingFormats.Webp;

    public double CompressionRatio { get; set; } = 60;

    public double ResizeScale { get; set; } = 100;

    public bool RoundedCorners { get; set; }

    public double CornerRadius { get; set; } = 14;

    public bool LuaBase64AutoWrap { get; set; }

    public int LuaBase64LineLength { get; set; } = 128;

    internal ImageEncodingOptions ToOptions() => new()
    {
        OutputFormat = OutputFormat,
        CompressionRatio = CompressionRatio,
        ResizeScale = ResizeScale,
        RoundedCorners = RoundedCorners,
        CornerRadius = CornerRadius,
        LuaBase64AutoWrap = LuaBase64AutoWrap,
        LuaBase64LineLength = LuaBase64LineLength,
    };
}
