namespace CatSit.Core.Animation;

public sealed class AnimationTimings
{
    public double WalkSpeedPxPerSec { get; init; } = 90;
    public int WalkFramePeriodMs { get; init; } = 110;
    public int SitDownDurationMs { get; init; } = 250;
    public int SittingDurationMs { get; init; } = 4000;
    public int LayDownDurationMs { get; init; } = 250;
    public int LayingDurationMs { get; init; } = 10000;
    public int SleepFramePeriodMs { get; init; } = 700;
    public int ZzzPeriodMs { get; init; } = 1500;
}
