using CatSit.Core.Learning;

namespace CatSit.Core.Detection;

public enum TriggerDecision
{
    Fire,
    ExactExclusion,
    LearnedSuppression
}

public sealed class TriggerFilter
{
    public const int UnlockGraceMs = 3000;

    private readonly ExclusionList _exclusions;
    private readonly CatClassifier _classifier;
    private readonly DetectionGate _gate = new();

    public TriggerFilter(ExclusionList exclusions, CatClassifier classifier)
    {
        _exclusions = exclusions;
        _classifier = classifier;
    }

    public bool DetectionEnabled { get; set; } = true;
    public bool LearningEnabled { get; set; } = true;
    public bool Locked { get; set; }
    public bool Suspended { get; set; }

    public bool AcceptsInput(long nowMs) =>
        !Locked && DetectionEnabled && !Suspended && _gate.IsOpen(nowMs);

    public TriggerDecision Evaluate(CatSignal signal)
    {
        if (_exclusions.IsExcluded(signal)) return TriggerDecision.ExactExclusion;
        if (LearningEnabled && _classifier.ShouldSuppress(signal.Features)) return TriggerDecision.LearnedSuppression;
        return TriggerDecision.Fire;
    }

    public void NotifyUnlocked(long nowMs) => _gate.StartGrace(nowMs, UnlockGraceMs);

    public void NotifySuppressed(long nowMs) => _gate.StartGrace(nowMs, UnlockGraceMs);
}
