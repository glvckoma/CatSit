namespace CatSit.Core.Detection;

public sealed record SensitivityProfile(
    int ConcurrentKeys,
    int BurstDistinctKeys,
    double BurstAdjacencyRatio,
    int BurstWindowMs,
    int ClickBurstCount,
    bool BothButtonsTriggers)
{
    private static readonly int[] ConcurrentByLevel = [6, 6, 5, 5, 4, 4, 4, 3, 3, 3];
    private static readonly int[] DistinctByLevel = [10, 10, 9, 9, 8, 8, 7, 7, 6, 6];
    private static readonly double[] RatioByLevel = [0.65, 0.62, 0.58, 0.54, 0.50, 0.47, 0.43, 0.40, 0.37, 0.35];
    private static readonly int[] WindowByLevel = [500, 550, 600, 650, 700, 750, 800, 850, 875, 900];
    private static readonly int[] ClicksByLevel = [9, 8, 8, 7, 6, 6, 5, 5, 4, 4];

    public static SensitivityProfile FromLevel(int level)
    {
        var index = Math.Clamp(level, 1, 10) - 1;
        return new SensitivityProfile(
            ConcurrentByLevel[index],
            DistinctByLevel[index],
            RatioByLevel[index],
            WindowByLevel[index],
            ClicksByLevel[index],
            index + 1 >= 4);
    }
}
