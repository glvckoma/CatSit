using CatSit.Core.Detection;
using CatSit.Core.Input;

namespace CatSit.Core.Tests;

public class KeyAdjacencyTests
{
    private static int Vk(char c) => char.ToUpperInvariant(c);

    [Fact]
    public void SameRowNeighbors_AreAdjacent()
    {
        Assert.True(KeyAdjacency.AreAdjacent(Vk('A'), Vk('S')));
        Assert.True(KeyAdjacency.AreAdjacent(Vk('Q'), Vk('W')));
        Assert.True(KeyAdjacency.AreAdjacent(Vk('N'), Vk('M')));
    }

    [Fact]
    public void CrossRowNeighbors_AreAdjacent()
    {
        Assert.True(KeyAdjacency.AreAdjacent(Vk('Q'), Vk('A')));
        Assert.True(KeyAdjacency.AreAdjacent(Vk('W'), Vk('S')));
        Assert.True(KeyAdjacency.AreAdjacent(Vk('G'), Vk('B')));
        Assert.True(KeyAdjacency.AreAdjacent('1', Vk('Q')));
    }

    [Fact]
    public void DistantKeys_AreNotAdjacent()
    {
        Assert.False(KeyAdjacency.AreAdjacent(Vk('A'), Vk('K')));
        Assert.False(KeyAdjacency.AreAdjacent(Vk('Q'), Vk('P')));
        Assert.False(KeyAdjacency.AreAdjacent(Vk('Z'), Vk('T')));
    }

    [Fact]
    public void SameKeyAndUnmappedKeys_AreNotAdjacent()
    {
        Assert.False(KeyAdjacency.AreAdjacent(Vk('A'), Vk('A')));
        Assert.False(KeyAdjacency.AreAdjacent(0x20, Vk('A')));
        Assert.False(KeyAdjacency.AreAdjacent(0x70, 0x71));
    }
}

public class KeyNamesTests
{
    [Fact]
    public void For_NamesLettersDigitsAndSpecials()
    {
        Assert.Equal("A", KeyNames.For(0x41));
        Assert.Equal("7", KeyNames.For(0x37));
        Assert.Equal("F5", KeyNames.For(0x74));
        Assert.Equal("Space", KeyNames.For(0x20));
    }

    [Fact]
    public void Signature_IsOrderIndependentAndDeduplicated()
    {
        Assert.Equal("A+S+D".Split('+').OrderBy(s => s).Aggregate((a, b) => a + "+" + b),
            KeyNames.Signature([0x53, 0x41, 0x44]));
        Assert.Equal(KeyNames.Signature([0x41, 0x53]), KeyNames.Signature([0x53, 0x41, 0x41]));
    }
}

public class SensitivityProfileTests
{
    [Fact]
    public void Level5_MatchesDefaults()
    {
        var p = SensitivityProfile.FromLevel(5);
        Assert.Equal(4, p.ConcurrentKeys);
        Assert.Equal(8, p.BurstDistinctKeys);
        Assert.Equal(0.5, p.BurstAdjacencyRatio, 2);
        Assert.Equal(700, p.BurstWindowMs);
        Assert.Equal(6, p.ClickBurstCount);
        Assert.True(p.BothButtonsTriggers);
        Assert.Equal(2600, p.HoldMsForTwoKeys);
    }

    [Fact]
    public void Level1_IsLax()
    {
        var p = SensitivityProfile.FromLevel(1);
        Assert.Equal(6, p.ConcurrentKeys);
        Assert.Equal(10, p.BurstDistinctKeys);
        Assert.Equal(500, p.BurstWindowMs);
        Assert.Equal(9, p.ClickBurstCount);
        Assert.False(p.BothButtonsTriggers);
        Assert.Equal(5000, p.HoldMsForTwoKeys);
    }

    [Fact]
    public void Level10_IsHairTrigger()
    {
        var p = SensitivityProfile.FromLevel(10);
        Assert.Equal(3, p.ConcurrentKeys);
        Assert.Equal(6, p.BurstDistinctKeys);
        Assert.Equal(900, p.BurstWindowMs);
        Assert.Equal(4, p.ClickBurstCount);
        Assert.Equal(1100, p.HoldMsForTwoKeys);
    }

    [Fact]
    public void OutOfRangeLevels_AreClamped()
    {
        Assert.Equal(SensitivityProfile.FromLevel(1), SensitivityProfile.FromLevel(0));
        Assert.Equal(SensitivityProfile.FromLevel(10), SensitivityProfile.FromLevel(99));
    }

    [Fact]
    public void RequiredHoldMs_ShrinksAsMoreKeysAreHeld()
    {
        var p = SensitivityProfile.FromLevel(5);
        Assert.Equal(2600, p.RequiredHoldMs(2, false));
        Assert.True(p.RequiredHoldMs(4, false) < p.RequiredHoldMs(2, false));
        Assert.True(p.RequiredHoldMs(6, false) < p.RequiredHoldMs(4, false));
    }

    [Fact]
    public void RequiredHoldMs_IsShorterForAdjacentKeys()
    {
        var p = SensitivityProfile.FromLevel(5);
        Assert.True(p.RequiredHoldMs(2, true) < p.RequiredHoldMs(2, false));
    }

    [Fact]
    public void RequiredHoldMs_NeverGoesBelowFloor()
    {
        var p = SensitivityProfile.FromLevel(10);
        Assert.True(p.RequiredHoldMs(12, true) >= SensitivityProfile.MinHoldMs);
    }
}

public class KeyboardCatDetectorTests
{
    private static KeyboardCatDetector Detector(int level = 5) => new(SensitivityProfile.FromLevel(level));

    private static KeyEventInfo Down(char c, long t, bool modHeld = false, bool injected = false) =>
        new(char.ToUpperInvariant(c), true, t, injected, modHeld);

    private static KeyEventInfo Up(char c, long t) => new(char.ToUpperInvariant(c), false, t, false, false);

    [Fact]
    public void PalmHoldingAdjacentCluster_TriggersPawCluster()
    {
        var detector = Detector();
        Assert.Null(detector.Observe(Down('A', 0)));
        Assert.Null(detector.Observe(Down('S', 30)));
        Assert.Null(detector.Observe(Down('D', 60)));
        var signal = detector.Observe(Down('F', 90));
        Assert.NotNull(signal);
        Assert.Equal(CatRule.PawCluster, signal.Rule);
        Assert.Equal("A+D+F+S", signal.Signature);
    }

    [Fact]
    public void FourScatteredKeysHeld_DoesNotTriggerPawCluster()
    {
        var detector = Detector();
        Assert.Null(detector.Observe(Down('Q', 0)));
        Assert.Null(detector.Observe(Down('P', 30)));
        Assert.Null(detector.Observe(Down('Z', 60)));
        Assert.Null(detector.Observe(Down('M', 90)));
    }

    [Fact]
    public void AdjacentRunOfQuickPresses_TriggersMashBurst()
    {
        var detector = Detector();
        var keys = "ASDFGHJK";
        CatSignal? signal = null;
        for (var i = 0; i < keys.Length; i++)
        {
            var t = i * 80L;
            signal ??= detector.Observe(Down(keys[i], t));
            detector.Observe(Up(keys[i], t + 40));
        }
        Assert.NotNull(signal);
        Assert.Equal(CatRule.MashBurst, signal.Rule);
    }

    [Fact]
    public void FastHumanTyping_DoesNotTrigger()
    {
        var detector = Detector();
        var keys = "EATONIRH";
        for (var i = 0; i < keys.Length; i++)
        {
            var t = i * 80L;
            Assert.Null(detector.Observe(Down(keys[i], t)));
            detector.Observe(Up(keys[i], t + 40));
        }
    }

    [Fact]
    public void TwoKeysHeldPastThreshold_TriggersSustainedHold()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0));
        detector.Observe(Down('L', 50));
        Assert.Null(detector.Poll(1000));
        Assert.Null(detector.Poll(2000));
        var signal = detector.Poll(2700);
        Assert.NotNull(signal);
        Assert.Equal(CatRule.SustainedHold, signal.Rule);
        Assert.Equal("A+L", signal.Signature);
    }

    [Fact]
    public void TwoAdjacentKeysHeld_TriggerSoonerThanScatteredKeys()
    {
        var adjacent = Detector();
        adjacent.Observe(Down('A', 0));
        adjacent.Observe(Down('S', 20));
        Assert.NotNull(adjacent.Poll(1900));

        var scattered = Detector();
        scattered.Observe(Down('A', 0));
        scattered.Observe(Down('L', 20));
        Assert.Null(scattered.Poll(1900));
    }

    [Fact]
    public void ManyKeysHeld_TriggerSoonerThanTwo()
    {
        var detector = Detector();
        detector.Observe(Down('Q', 0));
        detector.Observe(Down('T', 10));
        detector.Observe(Down('P', 20));
        detector.Observe(Down('Z', 30));
        Assert.Null(detector.Poll(1000));
        Assert.NotNull(detector.Poll(1400));
    }

    [Fact]
    public void SingleKeyHeldForever_DoesNotTriggerHold()
    {
        var detector = Detector(10);
        detector.Observe(Down('A', 0));
        Assert.Null(detector.Poll(30000));
    }

    [Fact]
    public void ReleasingBeforeThreshold_DoesNotTriggerHold()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0));
        detector.Observe(Down('L', 20));
        Assert.Null(detector.Poll(2000));
        detector.Observe(Up('A', 2100));
        Assert.Null(detector.Poll(9000));
    }

    [Fact]
    public void HoldRule_IgnoresModifierChords()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0, modHeld: true));
        detector.Observe(Down('S', 20, modHeld: true));
        Assert.Null(detector.Poll(9000));
    }

    [Fact]
    public void HoldTriggersOnlyOncePerHeldSet()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0));
        detector.Observe(Down('L', 20));
        Assert.NotNull(detector.Poll(3000));
        Assert.Null(detector.Poll(4000));
        Assert.Null(detector.Poll(9000));
    }

    [Fact]
    public void LowSensitivity_RequiresLongerHoldThanHighSensitivity()
    {
        var lax = Detector(1);
        lax.Observe(Down('A', 0));
        lax.Observe(Down('L', 20));
        Assert.Null(lax.Poll(3000));

        var twitchy = Detector(10);
        twitchy.Observe(Down('A', 0));
        twitchy.Observe(Down('L', 20));
        Assert.NotNull(twitchy.Poll(3000));
    }

    [Fact]
    public void Snapshot_ReportsHeldStateFeatures()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0));
        detector.Observe(Down('S', 100));
        var features = detector.Snapshot(1600);
        Assert.Equal(2, features.HeldKeyCount);
        Assert.Equal(1, features.HeldAdjacentPairs);
        Assert.InRange(features.MaxHoldMs, 1500, 1700);
    }

    [Fact]
    public void InjectedEvents_AreIgnored()
    {
        var detector = Detector();
        Assert.Null(detector.Observe(Down('A', 0, injected: true)));
        Assert.Null(detector.Observe(Down('S', 20, injected: true)));
        Assert.Null(detector.Observe(Down('D', 40, injected: true)));
        Assert.Null(detector.Observe(Down('F', 60, injected: true)));
        Assert.Null(detector.Poll(20000));
    }

    [Fact]
    public void AutoRepeatOfHeldKey_DoesNotAccumulate()
    {
        var detector = Detector();
        for (var i = 0; i < 20; i++)
        {
            Assert.Null(detector.Observe(Down('A', i * 30L)));
        }
    }

    [Fact]
    public void Reset_ClearsHeldKeys()
    {
        var detector = Detector();
        detector.Observe(Down('A', 0));
        detector.Observe(Down('S', 30));
        detector.Observe(Down('D', 60));
        detector.Reset();
        Assert.Null(detector.Observe(Down('F', 90)));
        Assert.Null(detector.Poll(20000));
    }
}

public class MouseCatDetectorTests
{
    private static MouseCatDetector Detector(int level = 5) => new(SensitivityProfile.FromLevel(level));

    private static MouseEventInfo Click(long t, MouseButton button = MouseButton.Left, bool injected = false) =>
        new(button, true, t, injected);

    private static MouseEventInfo Release(long t, MouseButton button = MouseButton.Left) =>
        new(button, false, t, false);

    [Fact]
    public void SixRapidClicks_TriggerClickBurst()
    {
        var detector = Detector();
        CatSignal? signal = null;
        for (var i = 0; i < 6; i++)
        {
            signal ??= detector.Observe(Click(i * 150L));
            detector.Observe(Release(i * 150L + 50));
        }
        Assert.NotNull(signal);
        Assert.Equal(CatRule.ClickBurst, signal.Rule);
    }

    [Fact]
    public void FiveRapidClicks_DoNotTrigger()
    {
        var detector = Detector();
        for (var i = 0; i < 5; i++)
        {
            Assert.Null(detector.Observe(Click(i * 150L)));
            detector.Observe(Release(i * 150L + 50));
        }
    }

    [Fact]
    public void SpreadOutClicks_DoNotTrigger()
    {
        var detector = Detector();
        for (var i = 0; i < 10; i++)
        {
            Assert.Null(detector.Observe(Click(i * 400L)));
            detector.Observe(Release(i * 400L + 50));
        }
    }

    [Fact]
    public void LeftAndRightHeldTogether_TriggersBothButtons()
    {
        var detector = Detector();
        Assert.Null(detector.Observe(Click(0, MouseButton.Left)));
        var signal = detector.Observe(Click(100, MouseButton.Right));
        Assert.NotNull(signal);
        Assert.Equal(CatRule.BothButtons, signal.Rule);
    }

    [Fact]
    public void BothButtonsRule_DisabledAtLowSensitivity()
    {
        var detector = Detector(level: 3);
        Assert.Null(detector.Observe(Click(0, MouseButton.Left)));
        Assert.Null(detector.Observe(Click(100, MouseButton.Right)));
    }

    [Fact]
    public void InjectedClicks_AreIgnored()
    {
        var detector = Detector();
        for (var i = 0; i < 10; i++)
        {
            Assert.Null(detector.Observe(Click(i * 50L, injected: true)));
        }
    }

    [Fact]
    public void Reset_ClearsClickWindow()
    {
        var detector = Detector();
        for (var i = 0; i < 5; i++)
        {
            detector.Observe(Click(i * 100L));
            detector.Observe(Release(i * 100L + 40));
        }
        detector.Reset();
        Assert.Null(detector.Observe(Click(500)));
    }
}

public class ExclusionListTests
{
    private static CatSignal Signal(params int[] keys) =>
        new(CatRule.SustainedHold, keys, new DetectionFeatures());

    [Fact]
    public void IsExcluded_TrueAfterAddingSignature()
    {
        var list = new ExclusionList();
        var signal = Signal(0x57, 0x44);
        Assert.False(list.IsExcluded(signal));
        list.Add(signal.Signature);
        Assert.True(list.IsExcluded(signal));
    }

    [Fact]
    public void IsExcluded_IgnoresKeyOrder()
    {
        var list = new ExclusionList();
        list.Add(Signal(0x44, 0x57).Signature);
        Assert.True(list.IsExcluded(Signal(0x57, 0x44)));
    }

    [Fact]
    public void IsExcluded_FalseForDifferentKeySet()
    {
        var list = new ExclusionList();
        list.Add(Signal(0x57, 0x44).Signature);
        Assert.False(list.IsExcluded(Signal(0x57, 0x41)));
    }

    [Fact]
    public void Remove_ReenablesSignature()
    {
        var list = new ExclusionList();
        var signal = Signal(0x57, 0x44);
        list.Add(signal.Signature);
        list.Remove(signal.Signature);
        Assert.False(list.IsExcluded(signal));
    }

    [Fact]
    public void Add_IsIdempotent()
    {
        var list = new ExclusionList();
        list.Add("W+D");
        list.Add("W+D");
        Assert.Single(list.Signatures);
    }

    [Fact]
    public void Load_ReplacesContents()
    {
        var list = new ExclusionList();
        list.Add("W+D");
        list.Load(["A+S", "Q+E"]);
        Assert.Equal(2, list.Signatures.Count);
        Assert.False(list.IsExcluded(Signal(0x57, 0x44)));
        Assert.True(list.IsExcluded(Signal(0x41, 0x53)));
    }
}

public class DetectionFeaturesTests
{
    [Fact]
    public void ToVector_NormalizesAndClampsToUnitRange()
    {
        var features = new DetectionFeatures
        {
            HeldKeyCount = 20,
            MaxHoldMs = 99999,
            HeldAdjacentPairs = 50,
            DistinctKeysInWindow = 40,
            BurstAdjacencyRatio = 1.0,
            KeysPerSecond = 100,
            ClicksPerSecond = 100,
            BothButtonsHeld = true
        };
        var vector = features.ToVector();
        Assert.Equal(8, vector.Length);
        Assert.All(vector, v => Assert.InRange(v, 0.0, 1.0));
    }

    [Fact]
    public void ToVector_EmptyFeatures_AreAllZero()
    {
        Assert.All(new DetectionFeatures().ToVector(), v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void ToVector_ScalesProportionally()
    {
        var half = new DetectionFeatures { HeldKeyCount = 4 }.ToVector()[0];
        var full = new DetectionFeatures { HeldKeyCount = 8 }.ToVector()[0];
        Assert.Equal(0.5, half, 2);
        Assert.Equal(1.0, full, 2);
    }
}
