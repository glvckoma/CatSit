namespace CatSit.Core.Detection;

public sealed class MouseCatDetector
{
    private readonly SensitivityProfile _profile;
    private readonly List<long> _clickTimes = [];
    private readonly HashSet<MouseButton> _held = [];

    public MouseCatDetector(SensitivityProfile profile)
    {
        _profile = profile;
    }

    public bool Observe(MouseEventInfo e)
    {
        if (e.Injected) return false;
        if (!e.IsDown)
        {
            _held.Remove(e.Button);
            return false;
        }
        _held.Add(e.Button);
        if (_profile.BothButtonsTriggers && _held.Contains(MouseButton.Left) && _held.Contains(MouseButton.Right)) return true;
        _clickTimes.Add(e.TimestampMs);
        _clickTimes.RemoveAll(t => e.TimestampMs - t > 1000);
        return _clickTimes.Count >= _profile.ClickBurstCount;
    }

    public void Reset()
    {
        _clickTimes.Clear();
        _held.Clear();
    }
}
