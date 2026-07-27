using CatSit.Core.Input;

namespace CatSit.Core.Detection;

public enum CatRule
{
    PawCluster,
    MashBurst,
    SustainedHold,
    ClickBurst,
    BothButtons
}

public sealed record DetectionFeatures
{
    private const double MaxHeldKeys = 8.0;
    private const double MaxHoldDurationMs = 4000.0;
    private const double MaxAdjacentPairs = 4.0;
    private const double MaxDistinctKeys = 12.0;
    private const double MaxKeysPerSecond = 15.0;
    private const double MaxClicksPerSecond = 10.0;

    public int HeldKeyCount { get; init; }
    public int MaxHoldMs { get; init; }
    public int HeldAdjacentPairs { get; init; }
    public int DistinctKeysInWindow { get; init; }
    public double BurstAdjacencyRatio { get; init; }
    public double KeysPerSecond { get; init; }
    public double ClicksPerSecond { get; init; }
    public bool BothButtonsHeld { get; init; }

    public double[] ToVector() =>
    [
        Normalize(HeldKeyCount, MaxHeldKeys),
        Normalize(MaxHoldMs, MaxHoldDurationMs),
        Normalize(HeldAdjacentPairs, MaxAdjacentPairs),
        Normalize(DistinctKeysInWindow, MaxDistinctKeys),
        Math.Clamp(BurstAdjacencyRatio, 0, 1),
        Normalize(KeysPerSecond, MaxKeysPerSecond),
        Normalize(ClicksPerSecond, MaxClicksPerSecond),
        BothButtonsHeld ? 1 : 0
    ];

    private static double Normalize(double value, double max) => Math.Clamp(value / max, 0, 1);
}

public sealed record CatSignal(CatRule Rule, IReadOnlyList<int> Keys, DetectionFeatures Features)
{
    public string Signature => KeyNames.Signature(Keys);
}
