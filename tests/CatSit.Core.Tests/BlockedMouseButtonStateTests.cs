using CatSit.Core.Detection;
using CatSit.Interop;

namespace CatSit.Core.Tests;

public class BlockedMouseButtonStateTests
{
    [Fact]
    public void BlockedDownAndMatchingUp_AreBothSwallowedAfterUnlock()
    {
        var state = new BlockedMouseButtonState();
        Assert.True(state.ShouldSwallow(MouseButton.Left, isDown: true, blocking: true));
        Assert.True(state.ShouldSwallow(MouseButton.Left, isDown: false, blocking: false));
        Assert.False(state.ShouldSwallow(MouseButton.Left, isDown: false, blocking: false));
    }

    [Fact]
    public void OutsideUnlockedClicks_AreNotSwallowed()
    {
        var state = new BlockedMouseButtonState();
        Assert.False(state.ShouldSwallow(MouseButton.Left, isDown: true, blocking: false));
        Assert.False(state.ShouldSwallow(MouseButton.Left, isDown: false, blocking: false));
    }

    [Fact]
    public void ButtonPairs_AreTrackedIndependently()
    {
        var state = new BlockedMouseButtonState();
        Assert.True(state.ShouldSwallow(MouseButton.Left, isDown: true, blocking: true));
        Assert.True(state.ShouldSwallow(MouseButton.Right, isDown: true, blocking: true));
        Assert.True(state.ShouldSwallow(MouseButton.Left, isDown: false, blocking: false));
        Assert.True(state.ShouldSwallow(MouseButton.Right, isDown: false, blocking: false));
    }
}
