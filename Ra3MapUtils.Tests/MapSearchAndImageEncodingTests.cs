using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Ra3MapUtils.API;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Impl;
using Ra3MapUtils.ViewModels.MainWindowPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ra3MapUtils.Tests;

[TestFixture]
public class MapSearchAndImageEncodingTests
{
    [TestCase("MyMap", "mymap")]
    [TestCase("RA3_Example", "ra3")]
    [TestCase("中文Map", "MAP")]
    public void MatchesSearchKeyword_IgnoresLetterCase(string mapName, string keyword)
    {
        Assert.That(MapManagePageViewModel.MatchesSearchKeyword(mapName, keyword), Is.True);
    }

    [Test]
    public void MatchesSearchKeyword_ReturnsAllMapsForEmptyKeyword()
    {
        Assert.That(MapManagePageViewModel.MatchesSearchKeyword("AnyMap", ""), Is.True);
    }

    [Test]
    public void MatchesSearchKeyword_RejectsUnrelatedNames()
    {
        Assert.That(MapManagePageViewModel.MatchesSearchKeyword("MyMap", "other"), Is.False);
    }

    [Test]
    public void ApplyRoundedCorners_ClearsCornersAndPreservesOriginalAlpha()
    {
        var service = new ImageEncodingService();
        var encoded = service.Encode(
            CreatePngBytes(20, 20, new Rgba32(255, 0, 0, 128)),
            new ImageEncodingOptions
            {
                OutputFormat = ImageEncodingFormats.Png,
                CompressionRatio = 0,
                RoundedCorners = true,
                CornerRadius = 6
            });

        using var result = Image.Load<Rgba32>(Convert.FromBase64String(encoded.Base64));
        Assert.Multiple(() =>
        {
            Assert.That(result[0, 0].A, Is.EqualTo(0));
            Assert.That(result[19, 0].A, Is.EqualTo(0));
            Assert.That(result[10, 10].A, Is.InRange(127, 128));
            Assert.That(result[0, 19].A, Is.EqualTo(0));
            Assert.That(result[19, 19].A, Is.EqualTo(0));
        });
    }

    [TestCase(20U, 10U, 99, 5)]
    [TestCase(20U, 10U, -2, 0)]
    [TestCase(20U, 10U, 3.6, 4)]
    public void ResolveCornerRadius_ClampsToImageBounds(uint width, uint height, double requested, int expected)
    {
        Assert.That(ImageEncodingService.ResolveCornerRadius(width, height, requested), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveCornerRadius_RejectsInvalidNumericInput()
    {
        Assert.That(ImageEncodingService.ResolveCornerRadius(20, 10, double.NaN), Is.Zero);
    }

    [Test]
    public void Preview_ReturnsOutputAndBase64SizeMetrics()
    {
        var sourceBytes = CreatePngBytes(20, 10, new Rgba32(10, 20, 30, 255));
        var result = new ImageEncodingService().Preview(
            sourceBytes,
            new ImageEncodingOptions
            {
                OutputFormat = ImageEncodingFormats.Png,
                CompressionRatio = 0,
                ResizeScale = 50
            });

        Assert.Multiple(() =>
        {
            Assert.That(result.OriginalWidth, Is.EqualTo(20));
            Assert.That(result.OriginalHeight, Is.EqualTo(10));
            Assert.That(result.OutputWidth, Is.EqualTo(10));
            Assert.That(result.OutputHeight, Is.EqualTo(5));
            Assert.That(result.SourceSizeBytes, Is.EqualTo(sourceBytes.Length));
            Assert.That(result.OutputSizeBytes, Is.GreaterThan(0));
            Assert.That(result.Base64SizeBytes, Is.EqualTo(((result.OutputSizeBytes + 2) / 3) * 4));
            Assert.That(result.GameMemorySizeBytes, Is.EqualTo(200));
        });
    }

    [Test]
    public async Task EncodeEndpoint_ReturnsWrappedEncodingResult()
    {
        var sourceBytes = CreatePngBytes(8, 8, new Rgba32(1, 2, 3, 255));
        using var fileStream = new MemoryStream(sourceBytes);
        var request = new ImageEncodingFormRequest
        {
            File = new FormFile(fileStream, 0, sourceBytes.Length, "file", "test.png"),
            OutputFormat = ImageEncodingFormats.Webp,
            LuaBase64AutoWrap = true,
            LuaBase64LineLength = 16
        };

        var response = await new ImageEncodingAPIService(new ImageEncodingService()).Encode(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo((int)ApiResponseCode.Success));
            Assert.That(response.Data.Base64, Is.Not.Empty);
            Assert.That(response.Data.Lua, Does.Contain("size = {8, 8}"));
            Assert.That(response.Data.OutputFormat, Is.EqualTo(ImageEncodingFormats.Webp));
        });
    }

    [Test]
    public async Task PreviewEndpoint_ReturnsWrappedSizeResult()
    {
        var sourceBytes = CreatePngBytes(12, 6, new Rgba32(4, 5, 6, 255));
        using var fileStream = new MemoryStream(sourceBytes);
        var request = new ImageEncodingFormRequest
        {
            File = new FormFile(fileStream, 0, sourceBytes.Length, "file", "preview.png"),
            OutputFormat = ImageEncodingFormats.Png,
            ResizeScale = 50
        };

        var response = await new ImageEncodingAPIService(new ImageEncodingService()).Preview(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo((int)ApiResponseCode.Success));
            Assert.That(response.Data.OutputWidth, Is.EqualTo(6));
            Assert.That(response.Data.OutputHeight, Is.EqualTo(3));
            Assert.That(response.Data.OutputSizeBytes, Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task EncodeEndpoint_ReportsInvalidImageAsIllegalArgument()
    {
        byte[] invalidImage = { 1, 2, 3, 4 };
        using var fileStream = new MemoryStream(invalidImage);
        var request = new ImageEncodingFormRequest
        {
            File = new FormFile(fileStream, 0, invalidImage.Length, "file", "invalid.png")
        };

        var response = await new ImageEncodingAPIService(new ImageEncodingService()).Encode(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo((int)ApiResponseCode.IllegalArgument));
            Assert.That(response.Message, Does.Contain("无法读取"));
        });
    }

    private static byte[] CreatePngBytes(int width, int height, Rgba32 color)
    {
        using var source = new Image<Rgba32>(width, height, color);
        using var stream = new MemoryStream();
        source.SaveAsPng(stream);
        return stream.ToArray();
    }
}
