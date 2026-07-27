using CatSit.Core.Animation;

namespace CatSit.Core.Tests;

public class CatAnimatorTests
{
    private const double ScreenWidth = 1000;
    private const double SpriteWidth = 128;
    private const double RestX = 400;

    private static CatAnimator StartedAnimator(bool fromLeft = true, long now = 0)
    {
        var animator = new CatAnimator();
        animator.Start(ScreenWidth, SpriteWidth, RestX, fromLeft, new AnimationTimings(), now);
        return animator;
    }

    private static CatAnimator SleepingAnimator(out long now)
    {
        var animator = StartedAnimator();
        now = 0;
        now += 20000;
        animator.Advance(now);
        now += 250;
        animator.Advance(now);
        now += 4000;
        animator.Advance(now);
        now += 250;
        animator.Advance(now);
        now += 10000;
        animator.Advance(now);
        return animator;
    }

    [Fact]
    public void Start_FromLeft_BeginsOffscreenWalkingRight()
    {
        var animator = StartedAnimator();
        Assert.Equal(CatState.WalkIn, animator.State);
        Assert.Equal(-SpriteWidth, animator.X);
        Assert.False(animator.FacingLeft);
        Assert.False(animator.ExitedScreen);
    }

    [Fact]
    public void Start_FromRight_BeginsAtRightEdgeFacingLeft()
    {
        var animator = StartedAnimator(fromLeft: false);
        Assert.Equal(ScreenWidth, animator.X);
        Assert.True(animator.FacingLeft);
    }

    [Fact]
    public void Advance_MovesTowardRestAtWalkSpeed()
    {
        var animator = StartedAnimator();
        animator.Advance(1000);
        Assert.Equal(-SpriteWidth + 90, animator.X, 3);
        Assert.Equal(CatState.WalkIn, animator.State);
    }

    [Fact]
    public void Advance_CyclesWalkFrames()
    {
        var animator = StartedAnimator();
        Assert.Equal(0, animator.FrameIndex);
        animator.Advance(110);
        Assert.Equal(1, animator.FrameIndex);
        animator.Advance(440);
        Assert.Equal(0, animator.FrameIndex);
    }

    [Fact]
    public void ReachingRest_SitsDownThenSitsThenLaysThenSleeps()
    {
        var animator = StartedAnimator();
        var now = 20000L;
        animator.Advance(now);
        Assert.Equal(RestX, animator.X);
        Assert.Equal(CatState.SitDown, animator.State);
        now += 250;
        animator.Advance(now);
        Assert.Equal(CatState.Sitting, animator.State);
        now += 4000;
        animator.Advance(now);
        Assert.Equal(CatState.LayDown, animator.State);
        now += 250;
        animator.Advance(now);
        Assert.Equal(CatState.Laying, animator.State);
        now += 10000;
        animator.Advance(now);
        Assert.Equal(CatState.Sleeping, animator.State);
    }

    [Fact]
    public void Sleeping_AlternatesBreathingFrames()
    {
        var animator = SleepingAnimator(out var now);
        Assert.Equal(0, animator.FrameIndex);
        animator.Advance(now + 700);
        Assert.Equal(1, animator.FrameIndex);
        animator.Advance(now + 1400);
        Assert.Equal(0, animator.FrameIndex);
    }

    [Fact]
    public void TryConsumeZzz_FiresOncePerPeriodWhileSleeping()
    {
        var animator = SleepingAnimator(out var now);
        Assert.False(animator.TryConsumeZzz(now + 100));
        Assert.True(animator.TryConsumeZzz(now + 1500));
        Assert.False(animator.TryConsumeZzz(now + 1600));
        Assert.True(animator.TryConsumeZzz(now + 3100));
    }

    [Fact]
    public void TryConsumeZzz_FalseWhileAwake()
    {
        var animator = StartedAnimator();
        animator.Advance(5000);
        Assert.False(animator.TryConsumeZzz(5000));
    }

    [Fact]
    public void Wake_WalksOutTowardNearestEdgeAndExits()
    {
        var animator = SleepingAnimator(out var now);
        animator.Wake(now);
        Assert.Equal(CatState.WalkOut, animator.State);
        Assert.True(animator.FacingLeft);
        animator.Advance(now + 2000);
        Assert.Equal(CatState.WalkOut, animator.State);
        animator.Advance(now + 20000);
        Assert.Equal(CatState.Hidden, animator.State);
        Assert.True(animator.ExitedScreen);
    }

    [Fact]
    public void Wake_NearRightEdge_ExitsRight()
    {
        var animator = new CatAnimator();
        animator.Start(ScreenWidth, SpriteWidth, 700, true, new AnimationTimings(), 0);
        animator.Advance(20000);
        animator.Wake(20000);
        Assert.False(animator.FacingLeft);
        animator.Advance(40000);
        Assert.True(animator.ExitedScreen);
    }

    [Fact]
    public void Wake_DuringWalkIn_TurnsAround()
    {
        var animator = StartedAnimator();
        animator.Advance(2000);
        animator.Wake(2000);
        Assert.Equal(CatState.WalkOut, animator.State);
    }
}
