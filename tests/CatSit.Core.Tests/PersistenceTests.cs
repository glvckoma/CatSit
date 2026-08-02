using System.Text.Json;
using CatSit.Core.Detection;
using CatSit.Core.Learning;
using CatSit.Core.Settings;
using CatSit.Settings;

namespace CatSit.Core.Tests;

public sealed class PersistenceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "CatSit.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SettingsStore_SaveAndLoad_RoundTripsAtomically()
    {
        var path = Path.Combine(_directory, "settings.json");
        var store = new SettingsStore(path);
        var saved = store.Save(new AppSettings { Sensitivity = 8, DetectMouseInput = false });
        var loaded = store.Load();
        Assert.True(saved.Success);
        Assert.True(loaded.Status.Success);
        Assert.Equal(8, loaded.Value.Sensitivity);
        Assert.False(loaded.Value.DetectMouseInput);
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
    }

    [Fact]
    public void SettingsStore_CorruptJson_ReturnsDefaultsAndFailure()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "settings.json");
        File.WriteAllText(path, "not json");
        var loaded = new SettingsStore(path).Load();
        Assert.False(loaded.Status.Success);
        Assert.True(loaded.Value.LockKeyboard);
    }

    [Fact]
    public void SettingsStore_UnwritablePath_ReturnsFailure()
    {
        Directory.CreateDirectory(_directory);
        var blocker = Path.Combine(_directory, "blocker");
        File.WriteAllText(blocker, "file blocks directory creation");
        var result = new SettingsStore(Path.Combine(blocker, "settings.json")).Save(new AppSettings());
        Assert.False(result.Success);
        Assert.Contains("could not save", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActivityStore_MigratesVersionOneSamplesToTriggerIds()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "activity.json");
        var features = new DetectionFeatures { HeldKeyCount = 5, HeldAdjacentPairs = 3 };
        var trigger = new TriggerRecord
        {
            Id = "trigger-1",
            TimestampUtc = DateTimeOffset.UtcNow,
            Rule = CatRule.PawCluster,
            Signature = "A+S+D",
            Locked = true,
            Verdict = TriggerVerdict.ConfirmedCat,
            Features = features
        };
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            Triggers = new[] { trigger },
            Weights = new double[CatClassifier.FeatureCount],
            Bias = 99.0,
            Samples = new[] { new { Features = features.ToVector(), IsCat = true } }
        }));

        var loaded = new ActivityStore(path).Load();
        Assert.True(loaded.Status.Success);
        Assert.Single(loaded.Value.Samples);
        Assert.Equal("trigger-1", loaded.Value.Samples[0].Id);
        Assert.True(loaded.Value.Samples[0].IsCat);
    }

    [Fact]
    public void ActivityStore_MigratesRepresentativeTwelveSampleHistory()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "activity.json");
        var samples = new[]
        {
            new LabeledSample { Features = [0, 0, 0, 0, 0, 0, 0.6, 0], IsCat = false },
            new LabeledSample { Features = [0.5, 0.0585, 0.75, 0.4167, 0.25, 0.4762, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.75, 0.1523, 0.25, 0.5, 0.2, 0.5714, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.5, 0, 0.5, 0.3333, 0.3333, 0.381, 0, 0], IsCat = true },
            new LabeledSample { Features = [0, 0, 0, 0, 0, 0, 0.1, 1], IsCat = false },
            new LabeledSample { Features = [0, 0, 0, 0, 0, 0, 0.1, 1], IsCat = false },
            new LabeledSample { Features = [0.5, 0.172, 0.5, 0.3333, 0.3333, 0.356, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.625, 0.144, 0.5, 0.4167, 0.25, 0.444, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.375, 1, 0, 0.0833, 0, 0.089, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.625, 0.004, 0.75, 0.4167, 0.5, 0.444, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.5, 0.004, 0.5, 0.4167, 0.5, 0.4762, 0, 0], IsCat = true },
            new LabeledSample { Features = [0.125, 0, 0, 0.75, 0.5, 0.8, 0, 0], IsCat = true }
        };
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            Triggers = Array.Empty<TriggerRecord>(),
            Weights = new double[CatClassifier.FeatureCount],
            Bias = -1.0,
            Samples = samples.Select(sample => new { sample.Features, sample.IsCat })
        }));

        var loaded = new ActivityStore(path).Load();
        var classifier = new CatClassifier();
        classifier.LoadSamples(loaded.Value.Samples);
        Assert.True(loaded.Status.Success);
        Assert.Equal(12, classifier.SampleCount);
        Assert.Equal(9, classifier.CatSampleCount);
        Assert.Equal(3, classifier.FalseAlarmSampleCount);
        Assert.True(classifier.IsActive);
    }

    [Fact]
    public void ActivityStore_VersionTwoSavesSamplesWithoutDerivedWeights()
    {
        var path = Path.Combine(_directory, "activity.json");
        var log = new TriggerLog();
        var classifier = new CatClassifier();
        classifier.SetLabel("sample-1", new DetectionFeatures { ClicksPerSecond = 6 }, false);
        var result = new ActivityStore(path).Save(log, classifier);
        var json = File.ReadAllText(path);
        Assert.True(result.Success);
        Assert.Contains("\"SchemaVersion\": 2", json);
        Assert.Contains("\"sample-1\"", json);
        Assert.DoesNotContain("\"Weights\"", json);
        Assert.DoesNotContain("\"Bias\"", json);
    }

    [Fact]
    public void ActivityStore_FutureVersionPreservesTriggersAndDropsSamples()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "activity.json");
        File.WriteAllText(path, """
            {
              "SchemaVersion": 99,
              "Triggers": [{ "Id": "future-trigger", "TimestampUtc": "2026-08-02T00:00:00Z" }],
              "Samples": [{ "Id": "future-sample", "Features": [0,0,0,0,0,0,0,0], "IsCat": true }]
            }
            """);
        var loaded = new ActivityStore(path).Load();
        Assert.False(loaded.Status.Success);
        Assert.Single(loaded.Value.Triggers);
        Assert.Empty(loaded.Value.Samples);
    }

    [Fact]
    public void StartupCommand_UsesBackgroundArgument()
    {
        Assert.Equal("\"C:\\Apps\\CatSit.exe\" --background", StartupRegistration.BuildCommand(@"C:\Apps\CatSit.exe"));
    }

    [Theory]
    [InlineData("--background", true)]
    [InlineData("--BACKGROUND", true)]
    [InlineData("--other", false)]
    public void LaunchOptions_ParseBackgroundMode(string argument, bool expected)
    {
        Assert.Equal(expected, AppLaunchOptions.Parse([argument]).Background);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
