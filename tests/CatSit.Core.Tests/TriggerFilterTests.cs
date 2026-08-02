using CatSit.Core.Detection;
using CatSit.Core.Learning;

namespace CatSit.Core.Tests;

public class TriggerFilterTests
{
    private static TriggerFilter Filter(ExclusionList? exclusions = null, CatClassifier? classifier = null) =>
        new(exclusions ?? new ExclusionList(), classifier ?? new CatClassifier());

    private static CatSignal HeldKeys(params int[] keys) =>
        new(CatRule.SustainedHold, keys.Length == 0 ? [0x57, 0x44] : keys, new DetectionFeatures { HeldKeyCount = 2 });

    [Theory]
    [InlineData(0L)]
    [InlineData(500L)]
    [InlineData(9_000_000L)]
    public void FreshFilter_AcceptsInputImmediately(long now)
    {
        Assert.True(Filter().AcceptsInput(now));
    }

    [Fact]
    public void LockedFilter_StopsAcceptingInput()
    {
        var filter = Filter();
        filter.Locked = true;
        Assert.False(filter.AcceptsInput(1000));
    }

    [Fact]
    public void DisabledDetection_StopsAcceptingInput()
    {
        var filter = Filter();
        filter.DetectionEnabled = false;
        Assert.False(filter.AcceptsInput(1000));
    }

    [Fact]
    public void SuspendedFilter_StopsAcceptingInput()
    {
        var filter = Filter();
        filter.Suspended = true;
        Assert.False(filter.AcceptsInput(1000));
    }

    [Fact]
    public void AfterUnlock_InputIsIgnoredForGracePeriodThenResumes()
    {
        var filter = Filter();
        filter.NotifyUnlocked(10_000);
        Assert.False(filter.AcceptsInput(11_000));
        Assert.True(filter.AcceptsInput(10_000 + TriggerFilter.UnlockGraceMs));
    }

    [Fact]
    public void Evaluate_FiresForNewSignal()
    {
        Assert.Equal(TriggerDecision.Fire, Filter().Evaluate(HeldKeys()));
    }

    [Fact]
    public void Evaluate_ReturnsExactExclusionForExcludedSignature()
    {
        var exclusions = new ExclusionList();
        exclusions.Add(HeldKeys().Signature);
        Assert.Equal(TriggerDecision.ExactExclusion, Filter(exclusions).Evaluate(HeldKeys()));
    }

    [Fact]
    public void Evaluate_ReturnsLearnedSuppressionWhenClassifierSuppresses()
    {
        var classifier = new CatClassifier();
        var features = new DetectionFeatures { HeldKeyCount = 2, MaxHoldMs = 2600, HeldAdjacentPairs = 1 };
        for (var i = 0; i < 6; i++) classifier.Learn(features, isCat: false);
        classifier.Learn(new DetectionFeatures { HeldKeyCount = 6 }, isCat: true);
        classifier.Learn(new DetectionFeatures { HeldKeyCount = 7 }, isCat: true);
        var signal = new CatSignal(CatRule.SustainedHold, [0x57, 0x44], features);
        Assert.Equal(TriggerDecision.LearnedSuppression, Filter(classifier: classifier).Evaluate(signal));
    }

    [Fact]
    public void Evaluate_IgnoresClassifierWhenLearningDisabled()
    {
        var classifier = new CatClassifier();
        var features = new DetectionFeatures { HeldKeyCount = 2, MaxHoldMs = 2600, HeldAdjacentPairs = 1 };
        for (var i = 0; i < 6; i++) classifier.Learn(features, isCat: false);
        classifier.Learn(new DetectionFeatures { HeldKeyCount = 6 }, isCat: true);
        classifier.Learn(new DetectionFeatures { HeldKeyCount = 7 }, isCat: true);
        var filter = Filter(classifier: classifier);
        filter.LearningEnabled = false;
        Assert.Equal(TriggerDecision.Fire,
            filter.Evaluate(new CatSignal(CatRule.SustainedHold, [0x57, 0x44], features)));
    }

    [Fact]
    public void AfterSuppression_InputIsIgnoredForGracePeriodThenResumes()
    {
        var filter = Filter();
        filter.NotifySuppressed(10_000);
        Assert.False(filter.AcceptsInput(11_000));
        Assert.True(filter.AcceptsInput(10_000 + TriggerFilter.UnlockGraceMs));
    }

    [Fact]
    public void HeldKeysAtStartup_SurviveTheWholePipeline()
    {
        var filter = Filter();
        var detector = new KeyboardCatDetector(SensitivityProfile.FromLevel(10));
        detector.Observe(new KeyEventInfo(0x41, true, 0, false, false));
        detector.Observe(new KeyEventInfo(0x4C, true, 10, false, false));
        Assert.True(filter.AcceptsInput(2000));
        var signal = detector.Poll(2000);
        Assert.NotNull(signal);
        Assert.Equal(TriggerDecision.Fire, filter.Evaluate(signal));
    }
}
