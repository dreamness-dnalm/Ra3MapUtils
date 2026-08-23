using System.Text.Json.Nodes;
using NUnit.Framework;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Impl;

namespace Ra3MapUtils.Tests;

[TestFixture]
public class DebuggerMapSettingsServiceTests
{
    private string _temporaryDirectory = null!;
    private string _settingsFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), "Ra3MapUtils.Tests", Guid.NewGuid().ToString("N"));
        var debuggerDirectory = Path.Combine(_temporaryDirectory, "data", "Ra3Hacker");
        Directory.CreateDirectory(debuggerDirectory);
        _settingsFilePath = Path.Combine(debuggerDirectory, "setting.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    [Test]
    public void Load_ReadsMapSettings()
    {
        File.WriteAllText(_settingsFilePath, """
            {
              "http": { "port": 30034 },
              "maps": {
                "folder": "",
                "hideBuiltIn": true
              }
            }
            """);

        var settings = new DebuggerMapSettingsService(_temporaryDirectory).Load();

        Assert.Multiple(() =>
        {
            Assert.That(settings.MapFolder, Is.Empty);
            Assert.That(settings.HideBuiltInMaps, Is.True);
        });
    }

    [Test]
    public void Save_UpdatesMapSettingsAndPreservesOtherProperties()
    {
        File.WriteAllText(_settingsFilePath, """
            {
              "http": { "port": 30034 },
              "custom": { "keep": "value" },
              "maps": {
                "folder": "",
                "hideBuiltIn": false,
                "futureOption": 7
              }
            }
            """);
        var mapDirectory = Path.Combine(_temporaryDirectory, "custom maps");
        Directory.CreateDirectory(mapDirectory);
        var service = new DebuggerMapSettingsService(_temporaryDirectory);

        service.Save(new DebuggerMapSettingsModel
        {
            MapFolder = mapDirectory,
            HideBuiltInMaps = true,
        });

        var root = JsonNode.Parse(File.ReadAllText(_settingsFilePath))!.AsObject();
        var maps = root["maps"]!.AsObject();
        Assert.Multiple(() =>
        {
            Assert.That(maps["folder"]!.GetValue<string>(), Is.EqualTo(Path.GetFullPath(mapDirectory)));
            Assert.That(maps["hideBuiltIn"]!.GetValue<bool>(), Is.True);
            Assert.That(maps["futureOption"]!.GetValue<int>(), Is.EqualTo(7));
            Assert.That(root["http"]!["port"]!.GetValue<int>(), Is.EqualTo(30034));
            Assert.That(root["custom"]!["keep"]!.GetValue<string>(), Is.EqualTo("value"));
        });
    }

    [Test]
    public void Save_AllowsEmptyFolderForGameDefault()
    {
        File.WriteAllText(_settingsFilePath, "{\"maps\":{\"folder\":\"old\",\"hideBuiltIn\":true}}");
        var service = new DebuggerMapSettingsService(_temporaryDirectory);

        service.Save(new DebuggerMapSettingsModel
        {
            MapFolder = "   ",
            HideBuiltInMaps = false,
        });

        var settings = service.Load();
        Assert.Multiple(() =>
        {
            Assert.That(settings.MapFolder, Is.Empty);
            Assert.That(settings.HideBuiltInMaps, Is.False);
        });
    }

    [Test]
    public void Save_RejectsMissingMapFolder()
    {
        File.WriteAllText(_settingsFilePath, "{}");
        var service = new DebuggerMapSettingsService(_temporaryDirectory);

        Assert.Throws<DirectoryNotFoundException>(() => service.Save(new DebuggerMapSettingsModel
        {
            MapFolder = Path.Combine(_temporaryDirectory, "missing"),
        }));
    }
}
