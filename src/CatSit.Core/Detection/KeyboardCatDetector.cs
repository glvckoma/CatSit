namespace CatSit.Core.Detection;

public sealed class KeyboardCatDetector
{
    private readonly SensitivityProfile _profile;
    private readonly Dictionary<int, long> _held = [];
    private readonly List<(int Vk, long TimestampMs)> _window = [];
    private string _reportedHoldSignature = string.Empty;

    public KeyboardCatDetector(SensitivityProfile profile)
    {
        _profile = profile;
    }

    public CatSignal? Observe(KeyEventInfo e)
    {
        if (e.Injected) return null;
        if (!e.IsDown)
        {
            _held.Remove(e.Vk);
            _reportedHoldSignature = string.Empty;
            return null;
        }
        if (e.CtrlOrAltHeld || IsModifier(e.Vk)) return null;
        if (!_held.TryAdd(e.Vk, e.TimestampMs)) return null;
        _reportedHoldSignature = string.Empty;
        _window.Add((e.Vk, e.TimestampMs));
        _window.RemoveAll(entry => e.TimestampMs - entry.TimestampMs > _profile.BurstWindowMs);
        if (HeldClusterLooksLikePaw())
        {
            return new CatSignal(CatRule.PawCluster, [.. _held.Keys], Snapshot(e.TimestampMs));
        }
        if (BurstLooksLikeCat())
        {
            return new CatSignal(CatRule.MashBurst, [.. _window.Select(entry => entry.Vk).Distinct()], Snapshot(e.TimestampMs));
        }
        return null;
    }

    public CatSignal? Poll(long nowMs)
    {
        if (_held.Count < SensitivityProfile.MinHoldKeys) return null;
        var keys = _held.Keys.ToArray();
        var signature = Core.Input.KeyNames.Signature(keys);
        if (signature == _reportedHoldSignature) return null;
        var required = _profile.RequiredHoldMs(keys.Length, AdjacentPairs(keys) > 0);
        if (LongestHoldMs(nowMs) < required) return null;
        _reportedHoldSignature = signature;
        return new CatSignal(CatRule.SustainedHold, keys, Snapshot(nowMs));
    }

    public DetectionFeatures Snapshot(long nowMs)
    {
        var keys = _held.Keys.ToArray();
        var windowSeconds = Math.Max(_profile.BurstWindowMs / 1000.0, 0.001);
        return new DetectionFeatures
        {
            HeldKeyCount = keys.Length,
            MaxHoldMs = (int)LongestHoldMs(nowMs),
            HeldAdjacentPairs = AdjacentPairs(keys),
            DistinctKeysInWindow = _window.Select(entry => entry.Vk).Distinct().Count(),
            BurstAdjacencyRatio = WindowAdjacencyRatio(),
            KeysPerSecond = _window.Count / windowSeconds
        };
    }

    public void Reset()
    {
        _held.Clear();
        _window.Clear();
        _reportedHoldSignature = string.Empty;
    }

    private long LongestHoldMs(long nowMs) => _held.Count == 0 ? 0 : _held.Values.Max(start => nowMs - start);

    private bool HeldClusterLooksLikePaw() =>
        _held.Count >= _profile.ConcurrentKeys && AdjacentPairs(_held.Keys.ToArray()) >= 2;

    private bool BurstLooksLikeCat() =>
        _window.Select(entry => entry.Vk).Distinct().Count() >= _profile.BurstDistinctKeys
        && WindowAdjacencyRatio() >= _profile.BurstAdjacencyRatio;

    private double WindowAdjacencyRatio()
    {
        var pairs = _window.Count - 1;
        if (pairs <= 0) return 0;
        var adjacent = 0;
        for (var i = 1; i < _window.Count; i++)
        {
            if (KeyAdjacency.AreAdjacent(_window[i - 1].Vk, _window[i].Vk)) adjacent++;
        }
        return (double)adjacent / pairs;
    }

    private static int AdjacentPairs(int[] keys)
    {
        var pairs = 0;
        for (var i = 0; i < keys.Length; i++)
        {
            for (var j = i + 1; j < keys.Length; j++)
            {
                if (KeyAdjacency.AreAdjacent(keys[i], keys[j])) pairs++;
            }
        }
        return pairs;
    }

    private static bool IsModifier(int vk) => vk is (>= 0xA0 and <= 0xA5) or 0x5B or 0x5C or 0x10 or 0x11 or 0x12;
}
