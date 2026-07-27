using CatSit.Core.Learning;

namespace CatSit.Core.Detection;

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

    public bool ShouldFire(CatSignal signal) =>
        !_exclusions.IsExcluded(signal)
        && !(LearningEnabled && _classifier.ShouldSuppress(signal.Features));

    public void NotifyUnlocked(long nowMs) => _gate.StartGrace(nowMs, UnlockGraceMs);
}
