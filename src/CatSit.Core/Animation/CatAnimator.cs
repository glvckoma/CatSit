namespace CatSit.Core.Animation;

public sealed class CatAnimator
{
    private AnimationTimings _timings = new();
    private double _screenWidth;
    private double _spriteWidth;
    private double _targetX;
    private long _stateEnteredMs;
    private long _lastAdvanceMs;
    private long _lastZzzMs;

    public CatState State { get; private set; } = CatState.Hidden;
    public double X { get; private set; }
    public bool FacingLeft { get; private set; }
    public int FrameIndex { get; private set; }
    public bool ExitedScreen { get; private set; }

    public void Start(double screenWidth, double spriteWidth, double restX, bool enterFromLeft, AnimationTimings timings, long nowMs)
    {
        _screenWidth = screenWidth;
        _spriteWidth = spriteWidth;
        _timings = timings;
        _targetX = restX;
        X = enterFromLeft ? -spriteWidth : screenWidth;
        FacingLeft = !enterFromLeft;
        ExitedScreen = false;
        _lastAdvanceMs = nowMs;
        EnterState(CatState.WalkIn, nowMs);
    }

    public void Wake(long nowMs)
    {
        if (State is CatState.Hidden or CatState.WalkOut) return;
        var exitLeft = X + _spriteWidth / 2 < _screenWidth / 2;
        _targetX = exitLeft ? -_spriteWidth : _screenWidth;
        FacingLeft = exitLeft;
        EnterState(CatState.WalkOut, nowMs);
    }

    public void Advance(long nowMs)
    {
        var dtMs = Math.Max(0, nowMs - _lastAdvanceMs);
        _lastAdvanceMs = nowMs;
        switch (State)
        {
            case CatState.WalkIn:
                MoveTowardTarget(dtMs);
                FrameIndex = WalkFrame(nowMs);
                if (X == _targetX) EnterState(CatState.SitDown, nowMs);
                break;
            case CatState.WalkOut:
                MoveTowardTarget(dtMs);
                FrameIndex = WalkFrame(nowMs);
                if (X == _targetX)
                {
                    EnterState(CatState.Hidden, nowMs);
                    ExitedScreen = true;
                }
                break;
            case CatState.SitDown:
                if (Elapsed(nowMs) >= _timings.SitDownDurationMs) EnterState(CatState.Sitting, nowMs);
                break;
            case CatState.Sitting:
                if (Elapsed(nowMs) >= _timings.SittingDurationMs) EnterState(CatState.LayDown, nowMs);
                break;
            case CatState.LayDown:
                if (Elapsed(nowMs) >= _timings.LayDownDurationMs) EnterState(CatState.Laying, nowMs);
                break;
            case CatState.Laying:
                if (Elapsed(nowMs) >= _timings.LayingDurationMs)
                {
                    EnterState(CatState.Sleeping, nowMs);
                    _lastZzzMs = nowMs;
                }
                break;
            case CatState.Sleeping:
                FrameIndex = (int)(Elapsed(nowMs) / _timings.SleepFramePeriodMs) % 2;
                break;
        }
    }

    public bool TryConsumeZzz(long nowMs)
    {
        if (State != CatState.Sleeping) return false;
        if (nowMs - _lastZzzMs < _timings.ZzzPeriodMs) return false;
        _lastZzzMs = nowMs;
        return true;
    }

    private void EnterState(CatState state, long nowMs)
    {
        State = state;
        _stateEnteredMs = nowMs;
        FrameIndex = 0;
    }

    private long Elapsed(long nowMs) => nowMs - _stateEnteredMs;

    private int WalkFrame(long nowMs) => (int)(Elapsed(nowMs) / _timings.WalkFramePeriodMs) % 4;

    private void MoveTowardTarget(long dtMs)
    {
        var step = _timings.WalkSpeedPxPerSec * dtMs / 1000.0;
        if (X < _targetX) X = Math.Min(_targetX, X + step);
        else if (X > _targetX) X = Math.Max(_targetX, X - step);
    }
}
