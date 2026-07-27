namespace CatSit.Core.Detection;

public sealed class KeyboardCatDetector
{
    private readonly SensitivityProfile _profile;
    private readonly HashSet<int> _held = [];
    private readonly List<(int Vk, long TimestampMs)> _window = [];

    public KeyboardCatDetector(SensitivityProfile profile)
    {
        _profile = profile;
    }

    public bool Observe(KeyEventInfo e)
    {
        if (e.Injected) return false;
        if (!e.IsDown)
        {
            _held.Remove(e.Vk);
            return false;
        }
        if (e.CtrlOrAltHeld || IsModifier(e.Vk)) return false;
        if (!_held.Add(e.Vk)) return false;
        _window.Add((e.Vk, e.TimestampMs));
        _window.RemoveAll(entry => e.TimestampMs - entry.TimestampMs > _profile.BurstWindowMs);
        return HeldClusterLooksLikePaw() || BurstLooksLikeCat();
    }

    public void Reset()
    {
        _held.Clear();
        _window.Clear();
    }

    private bool HeldClusterLooksLikePaw()
    {
        if (_held.Count < _profile.ConcurrentKeys) return false;
        var keys = _held.ToArray();
        var adjacentPairs = 0;
        for (var i = 0; i < keys.Length; i++)
        {
            for (var j = i + 1; j < keys.Length; j++)
            {
                if (KeyAdjacency.AreAdjacent(keys[i], keys[j])) adjacentPairs++;
            }
        }
        return adjacentPairs >= 2;
    }

    private bool BurstLooksLikeCat()
    {
        if (_window.Select(entry => entry.Vk).Distinct().Count() < _profile.BurstDistinctKeys) return false;
        var adjacent = 0;
        var pairs = _window.Count - 1;
        for (var i = 1; i < _window.Count; i++)
        {
            if (KeyAdjacency.AreAdjacent(_window[i - 1].Vk, _window[i].Vk)) adjacent++;
        }
        return pairs > 0 && (double)adjacent / pairs >= _profile.BurstAdjacencyRatio;
    }

    private static bool IsModifier(int vk) => vk is (>= 0xA0 and <= 0xA5) or 0x5B or 0x5C or 0x10 or 0x11 or 0x12;
}
