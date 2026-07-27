using CatSit.Core.Detection;

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
    }

    [Fact]
    public void Level1_IsLax()
    {
        var p = SensitivityProfile.FromLevel(1);
        Assert.Equal(6, p.ConcurrentKeys);
        Assert.Equal(10, p.BurstDistinctKeys);
        Assert.Equal(0.65, p.BurstAdjacencyRatio, 2);
        Assert.Equal(500, p.BurstWindowMs);
        Assert.Equal(9, p.ClickBurstCount);
        Assert.False(p.BothButtonsTriggers);
    }

    [Fact]
    public void Level10_IsHairTrigger()
    {
        var p = SensitivityProfile.FromLevel(10);
        Assert.Equal(3, p.ConcurrentKeys);
        Assert.Equal(6, p.BurstDistinctKeys);
        Assert.Equal(0.35, p.BurstAdjacencyRatio, 2);
        Assert.Equal(900, p.BurstWindowMs);
        Assert.Equal(4, p.ClickBurstCount);
    }

    [Fact]
    public void OutOfRangeLevels_AreClamped()
    {
        Assert.Equal(SensitivityProfile.FromLevel(1), SensitivityProfile.FromLevel(0));
        Assert.Equal(SensitivityProfile.FromLevel(10), SensitivityProfile.FromLevel(99));
    }
}

public class KeyboardCatDetectorTests
{
    private static KeyboardCatDetector Detector() => new(SensitivityProfile.FromLevel(5));

    private static KeyEventInfo Down(char c, long t, bool modHeld = false, bool injected = false) =>
        new(char.ToUpperInvariant(c), true, t, injected, modHeld);

    private static KeyEventInfo Up(char c, long t) => new(char.ToUpperInvariant(c), false, t, false, false);

    [Fact]
    public void PalmHoldingAdjacentCluster_Triggers()
    {
        var detector = Detector();
        Assert.False(detector.Observe(Down('A', 0)));
        Assert.False(detector.Observe(Down('S', 30)));
        Assert.False(detector.Observe(Down('D', 60)));
        Assert.True(detector.Observe(Down('F', 90)));
    }

    [Fact]
    public void FourScatteredKeysHeld_DoesNotTriggerSimultaneityRule()
    {
        var detector = Detector();
        Assert.False(detector.Observe(Down('Q', 0)));
        Assert.False(detector.Observe(Down('P', 30)));
        Assert.False(detector.Observe(Down('Z', 60)));
        Assert.False(detector.Observe(Down('M', 90)));
    }

    [Fact]
    public void AdjacentRunOfQuickPresses_TriggersBurstRule()
    {
        var detector = Detector();
        var keys = "ASDFGHJK";
        var triggered = false;
        for (var i = 0; i < keys.Length; i++)
        {
            var t = i * 80L;
            triggered |= detector.Observe(Down(keys[i], t));
            detector.Observe(Up(keys[i], t + 40));
        }
        Assert.True(triggered);
    }

    [Fact]
    public void FastHumanTyping_DoesNotTrigger()
    {
        var detector = Detector();
        var keys = "EATONIRH";
        var triggered = false;
        for (var i = 0; i < keys.Length; i++)
        {
            var t = i * 80L;
            triggered |= detector.Observe(Down(keys[i], t));
            detector.Observe(Up(keys[i], t + 40));
        }
        Assert.False(triggered);
    }

    [Fact]
    public void SlowAdjacentTyping_DoesNotTrigger()
    {
        var detector = Detector();
        var keys = "ASDFGHJK";
        var triggered = false;
        for (var i = 0; i < keys.Length; i++)
        {
            var t = i * 300L;
            triggered |= detector.Observe(Down(keys[i], t));
            detector.Observe(Up(keys[i], t + 100));
        }
        Assert.False(triggered);
    }

    [Fact]
    public void ModifierChords_AreIgnored()
    {
        var detector = Detector();
        Assert.False(detector.Observe(Down('A', 0, modHeld: true)));
        Assert.False(detector.Observe(Down('S', 20, modHeld: true)));
        Assert.False(detector.Observe(Down('D', 40, modHeld: true)));
        Assert.False(detector.Observe(Down('F', 60, modHeld: true)));
    }

    [Fact]
    public void InjectedEvents_AreIgnored()
    {
        var detector = Detector();
        Assert.False(detector.Observe(Down('A', 0, injected: true)));
        Assert.False(detector.Observe(Down('S', 20, injected: true)));
        Assert.False(detector.Observe(Down('D', 40, injected: true)));
        Assert.False(detector.Observe(Down('F', 60, injected: true)));
    }

    [Fact]
    public void AutoRepeatOfHeldKey_DoesNotAccumulate()
    {
        var detector = Detector();
        for (var i = 0; i < 20; i++)
        {
            Assert.False(detector.Observe(Down('A', i * 30L)));
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
        Assert.False(detector.Observe(Down('F', 90)));
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
    public void SixRapidClicks_Trigger()
    {
        var detector = Detector();
        var triggered = false;
        for (var i = 0; i < 6; i++)
        {
            triggered |= detector.Observe(Click(i * 150L));
            detector.Observe(Release(i * 150L + 50));
        }
        Assert.True(triggered);
    }

    [Fact]
    public void FiveRapidClicks_DoNotTrigger()
    {
        var detector = Detector();
        var triggered = false;
        for (var i = 0; i < 5; i++)
        {
            triggered |= detector.Observe(Click(i * 150L));
            detector.Observe(Release(i * 150L + 50));
        }
        Assert.False(triggered);
    }

    [Fact]
    public void SpreadOutClicks_DoNotTrigger()
    {
        var detector = Detector();
        var triggered = false;
        for (var i = 0; i < 10; i++)
        {
            triggered |= detector.Observe(Click(i * 400L));
            detector.Observe(Release(i * 400L + 50));
        }
        Assert.False(triggered);
    }

    [Fact]
    public void LeftAndRightHeldTogether_Triggers()
    {
        var detector = Detector();
        Assert.False(detector.Observe(Click(0, MouseButton.Left)));
        Assert.True(detector.Observe(Click(100, MouseButton.Right)));
    }

    [Fact]
    public void BothButtonsRule_DisabledAtLowSensitivity()
    {
        var detector = Detector(level: 3);
        Assert.False(detector.Observe(Click(0, MouseButton.Left)));
        Assert.False(detector.Observe(Click(100, MouseButton.Right)));
    }

    [Fact]
    public void InjectedClicks_AreIgnored()
    {
        var detector = Detector();
        var triggered = false;
        for (var i = 0; i < 10; i++)
        {
            triggered |= detector.Observe(Click(i * 50L, injected: true));
        }
        Assert.False(triggered);
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
        Assert.False(detector.Observe(Click(500)));
    }
}
