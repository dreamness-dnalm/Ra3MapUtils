using ImageMagick;
using NUnit.Framework;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.ViewModels.toolbox;
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
        using var source = new Image<Rgba32>(20, 20, new Rgba32(255, 0, 0, 128));
        using var stream = new MemoryStream();
        source.SaveAsPng(stream);
        stream.Position = 0;

        using var image = new MagickImage(stream);
        ImageEncodingToolWindowViewModel.ApplyRoundedCorners(image, 6);

        using var result = Image.Load<Rgba32>(image.ToByteArray(MagickFormat.Png));
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
        Assert.That(ImageEncodingToolWindowViewModel.ResolveCornerRadius(width, height, requested), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveCornerRadius_RejectsInvalidNumericInput()
    {
        Assert.That(ImageEncodingToolWindowViewModel.ResolveCornerRadius(20, 10, double.NaN), Is.Zero);
    }
}
