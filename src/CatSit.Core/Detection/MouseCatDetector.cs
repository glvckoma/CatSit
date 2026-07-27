namespace CatSit.Core.Detection;

public sealed class MouseCatDetector
{
    private const int ClickWindowMs = 1000;

    private readonly SensitivityProfile _profile;
    private readonly List<long> _clickTimes = [];
    private readonly HashSet<MouseButton> _held = [];

    public MouseCatDetector(SensitivityProfile profile)
    {
        _profile = profile;
    }

    public CatSignal? Observe(MouseEventInfo e)
    {
        if (e.Injected) return null;
        if (!e.IsDown)
        {
            _held.Remove(e.Button);
            return null;
        }
        _held.Add(e.Button);
        var bothHeld = _held.Contains(MouseButton.Left) && _held.Contains(MouseButton.Right);
        if (_profile.BothButtonsTriggers && bothHeld)
        {
            return new CatSignal(CatRule.BothButtons, [], Features(bothHeld));
        }
        _clickTimes.Add(e.TimestampMs);
        _clickTimes.RemoveAll(t => e.TimestampMs - t > ClickWindowMs);
        if (_clickTimes.Count >= _profile.ClickBurstCount)
        {
            return new CatSignal(CatRule.ClickBurst, [], Features(bothHeld));
        }
        return null;
    }

    public void Reset()
    {
        _clickTimes.Clear();
        _held.Clear();
    }

    private DetectionFeatures Features(bool bothHeld) => new()
    {
        ClicksPerSecond = _clickTimes.Count * 1000.0 / ClickWindowMs,
        BothButtonsHeld = bothHeld
    };
}
