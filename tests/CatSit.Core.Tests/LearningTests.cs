using CatSit.Core.Detection;
using CatSit.Core.Learning;

namespace CatSit.Core.Tests;

public class TriggerLogTests
{
    private static readonly DateTimeOffset Start = new(2026, 7, 27, 12, 0, 0, TimeSpan.Zero);

    private static CatSignal Signal(CatRule rule = CatRule.SustainedHold, params int[] keys) =>
        new(rule, keys.Length == 0 ? [0x57, 0x44] : keys, new DetectionFeatures { HeldKeyCount = 2 });

    [Fact]
    public void Add_StoresRecordWithSignatureAndRule()
    {
        var log = new TriggerLog();
        var record = log.Add(Signal(), locked: true, Start, "id-1");
        Assert.Equal("id-1", record.Id);
        Assert.Equal(CatRule.SustainedHold, record.Rule);
        Assert.Equal("D+W", record.Signature);
        Assert.True(record.Locked);
        Assert.Equal(TriggerVerdict.Unreviewed, record.Verdict);
        Assert.Single(log.Entries);
    }

    [Fact]
    public void Entries_AreNewestFirst()
    {
        var log = new TriggerLog();
        log.Add(Signal(), true, Start, "old");
        log.Add(Signal(), true, Start.AddMinutes(1), "new");
        Assert.Equal("new", log.Entries[0].Id);
        Assert.Equal("old", log.Entries[1].Id);
    }

    [Fact]
    public void Add_EvictsOldestBeyondMaxEntries()
    {
        var log = new TriggerLog();
        for (var i = 0; i < TriggerLog.MaxEntries + 10; i++)
        {
            log.Add(Signal(), true, Start.AddSeconds(i), $"id-{i}");
        }
        Assert.Equal(TriggerLog.MaxEntries, log.Entries.Count);
        Assert.Equal($"id-{TriggerLog.MaxEntries + 9}", log.Entries[0].Id);
        Assert.DoesNotContain(log.Entries, r => r.Id == "id-0");
    }

    [Fact]
    public void SetVerdict_UpdatesRecord()
    {
        var log = new TriggerLog();
        log.Add(Signal(), true, Start, "id-1");
        var updated = log.SetVerdict("id-1", TriggerVerdict.FalseAlarm);
        Assert.NotNull(updated);
        Assert.Equal(TriggerVerdict.FalseAlarm, updated.Verdict);
        Assert.Equal(TriggerVerdict.FalseAlarm, log.Entries[0].Verdict);
    }

    [Fact]
    public void SetVerdict_UnknownId_ReturnsNull()
    {
        var log = new TriggerLog();
        Assert.Null(log.SetVerdict("missing", TriggerVerdict.ConfirmedCat));
    }

    [Fact]
    public void Load_ReplacesEntriesAndKeepsNewestFirst()
    {
        var log = new TriggerLog();
        log.Add(Signal(), true, Start, "old");
        log.Load([
            new TriggerRecord { Id = "a", TimestampUtc = Start },
            new TriggerRecord { Id = "b", TimestampUtc = Start.AddMinutes(5) }
        ]);
        Assert.Equal(2, log.Entries.Count);
        Assert.Equal("b", log.Entries[0].Id);
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        var log = new TriggerLog();
        log.Add(Signal(), true, Start, "id-1");
        log.Clear();
        Assert.Empty(log.Entries);
    }
}

public class CatClassifierTests
{
    private static DetectionFeatures CatLike() => new()
    {
        HeldKeyCount = 5,
        MaxHoldMs = 3000,
        HeldAdjacentPairs = 3,
        DistinctKeysInWindow = 9,
        BurstAdjacencyRatio = 0.7,
        KeysPerSecond = 10
    };

    private static DetectionFeatures HumanLike() => new()
    {
        HeldKeyCount = 2,
        MaxHoldMs = 2600,
        HeldAdjacentPairs = 1,
        DistinctKeysInWindow = 2,
        BurstAdjacencyRatio = 0.1,
        KeysPerSecond = 2
    };

    [Fact]
    public void Score_IsProbabilityBetweenZeroAndOne()
    {
        var classifier = new CatClassifier();
        Assert.InRange(classifier.Score(CatLike()), 0.0, 1.0);
        Assert.InRange(classifier.Score(new DetectionFeatures()), 0.0, 1.0);
    }

    [Fact]
    public void UntrainedClassifier_RatesCatLikeHigherThanEmpty()
    {
        var classifier = new CatClassifier();
        Assert.True(classifier.Score(CatLike()) > classifier.Score(new DetectionFeatures()));
    }

    [Fact]
    public void UntrainedClassifier_IsInactiveAndNeverSuppresses()
    {
        var classifier = new CatClassifier();
        Assert.False(classifier.IsActive);
        Assert.False(classifier.ShouldSuppress(HumanLike()));
        Assert.False(classifier.ShouldSuppress(new DetectionFeatures()));
    }

    [Fact]
    public void BecomesActiveAfterMinimumSamples()
    {
        var classifier = new CatClassifier();
        for (var i = 0; i < CatClassifier.MinSamplesBeforeVeto; i++)
        {
            classifier.Learn(HumanLike(), isCat: false);
        }
        Assert.True(classifier.IsActive);
        Assert.Equal(CatClassifier.MinSamplesBeforeVeto, classifier.SampleCount);
    }

    [Fact]
    public void LearningFalseAlarms_LowersScoreForThatPattern()
    {
        var classifier = new CatClassifier();
        var before = classifier.Score(HumanLike());
        for (var i = 0; i < 5; i++) classifier.Learn(HumanLike(), isCat: false);
        Assert.True(classifier.Score(HumanLike()) < before);
    }

    [Fact]
    public void LearningFalseAlarms_LeadsToSuppression()
    {
        var classifier = new CatClassifier();
        for (var i = 0; i < 6; i++) classifier.Learn(HumanLike(), isCat: false);
        Assert.True(classifier.ShouldSuppress(HumanLike()));
    }

    [Fact]
    public void ConfirmedCats_StayUnsuppressedWhileFalseAlarmsAreLearned()
    {
        var classifier = new CatClassifier();
        for (var i = 0; i < 6; i++)
        {
            classifier.Learn(HumanLike(), isCat: false);
            classifier.Learn(CatLike(), isCat: true);
        }
        Assert.False(classifier.ShouldSuppress(CatLike()));
        Assert.True(classifier.ShouldSuppress(HumanLike()));
    }

    [Fact]
    public void Learn_IsDeterministic()
    {
        var a = new CatClassifier();
        var b = new CatClassifier();
        for (var i = 0; i < 4; i++)
        {
            a.Learn(HumanLike(), false);
            b.Learn(HumanLike(), false);
        }
        Assert.Equal(a.Score(CatLike()), b.Score(CatLike()), 10);
    }

    [Fact]
    public void SampleHistory_IsCappedAndKeepsRecent()
    {
        var classifier = new CatClassifier();
        for (var i = 0; i < CatClassifier.MaxSamples + 15; i++)
        {
            classifier.Learn(HumanLike(), isCat: i % 2 == 0);
        }
        Assert.Equal(CatClassifier.MaxSamples, classifier.SampleCount);
        Assert.Equal(CatClassifier.MaxSamples, classifier.Samples.Count);
    }

    [Fact]
    public void Load_RestoresPersistedStateAndScores()
    {
        var original = new CatClassifier();
        for (var i = 0; i < 5; i++) original.Learn(HumanLike(), false);
        var restored = new CatClassifier();
        restored.Load(original.Weights, original.Bias, original.Samples);
        Assert.Equal(original.Score(HumanLike()), restored.Score(HumanLike()), 10);
        Assert.Equal(original.SampleCount, restored.SampleCount);
    }

    [Fact]
    public void Load_WithWrongWeightCount_KeepsDefaults()
    {
        var classifier = new CatClassifier();
        var expected = classifier.Score(CatLike());
        classifier.Load([1.0, 2.0], 5.0, []);
        Assert.Equal(expected, classifier.Score(CatLike()), 10);
    }

    [Fact]
    public void Reset_ReturnsToUntrainedState()
    {
        var classifier = new CatClassifier();
        var pristine = classifier.Score(HumanLike());
        for (var i = 0; i < 6; i++) classifier.Learn(HumanLike(), false);
        classifier.Reset();
        Assert.False(classifier.IsActive);
        Assert.Equal(0, classifier.SampleCount);
        Assert.Equal(pristine, classifier.Score(HumanLike()), 10);
    }
}
