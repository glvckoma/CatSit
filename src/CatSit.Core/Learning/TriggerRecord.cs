using CatSit.Core.Detection;

namespace CatSit.Core.Learning;

public enum TriggerVerdict
{
    Unreviewed,
    ConfirmedCat,
    FalseAlarm
}

public sealed record TriggerRecord
{
    public string Id { get; init; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; init; }
    public CatRule Rule { get; init; }
    public string Signature { get; init; } = string.Empty;
    public bool Locked { get; init; }
    public TriggerVerdict Verdict { get; init; } = TriggerVerdict.Unreviewed;
    public DetectionFeatures Features { get; init; } = new();

    public string RuleDescription => Rule switch
    {
        CatRule.PawCluster => "Paw on several keys",
        CatRule.MashBurst => "Key mashing",
        CatRule.SustainedHold => "Keys held down",
        CatRule.ClickBurst => "Rapid clicking",
        CatRule.BothButtons => "Both mouse buttons",
        _ => Rule.ToString()
    };
}
