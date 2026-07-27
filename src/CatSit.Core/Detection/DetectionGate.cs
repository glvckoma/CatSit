namespace CatSit.Core.Detection;

public sealed class DetectionGate
{
    private long _openAtMs;

    public void StartGrace(long nowMs, int graceMs) =>
        _openAtMs = graceMs > 0 && nowMs > long.MaxValue - graceMs ? long.MaxValue : nowMs + graceMs;

    public bool IsOpen(long nowMs) => nowMs >= _openAtMs;

    public void Open() => _openAtMs = 0;
}
