using CatSit.Core.Hotkeys;

namespace CatSit.Core.Tests;

public class ModifierTrackerTests
{
    [Fact]
    public void Update_TracksLeftCtrlDownAndUp()
    {
        var state = ModifierTracker.Update(0, 0xA2, true);
        Assert.Equal(ModifierTracker.LCtrl, state);
        state = ModifierTracker.Update(state, 0xA2, false);
        Assert.Equal(0, state);
    }

    [Fact]
    public void Update_AccumulatesMultipleModifiers()
    {
        var state = ModifierTracker.Update(0, 0xA2, true);
        state = ModifierTracker.Update(state, 0xA4, true);
        state = ModifierTracker.Update(state, 0xA1, true);
        Assert.Equal(ModifierTracker.LCtrl | ModifierTracker.LAlt | ModifierTracker.RShift, state);
    }

    [Fact]
    public void Update_TracksRightAltAndWinKeys()
    {
        var state = ModifierTracker.Update(0, 0xA5, true);
        state = ModifierTracker.Update(state, 0x5B, true);
        state = ModifierTracker.Update(state, 0x5C, true);
        Assert.Equal(ModifierTracker.RAlt | ModifierTracker.LWin | ModifierTracker.RWin, state);
    }

    [Fact]
    public void Update_IgnoresNonModifierKeys()
    {
        var state = ModifierTracker.Update(0, 0x41, true);
        Assert.Equal(0, state);
    }

    [Fact]
    public void Update_KeyUpWithoutDown_StaysClear()
    {
        var state = ModifierTracker.Update(0, 0xA3, false);
        Assert.Equal(0, state);
    }

    [Fact]
    public void CtrlOrAltHeld_TrueForAnyCtrlOrAlt()
    {
        Assert.True(ModifierTracker.CtrlOrAltHeld(ModifierTracker.LCtrl));
        Assert.True(ModifierTracker.CtrlOrAltHeld(ModifierTracker.RCtrl));
        Assert.True(ModifierTracker.CtrlOrAltHeld(ModifierTracker.LAlt));
        Assert.True(ModifierTracker.CtrlOrAltHeld(ModifierTracker.RAlt));
        Assert.False(ModifierTracker.CtrlOrAltHeld(ModifierTracker.LShift | ModifierTracker.LWin));
        Assert.False(ModifierTracker.CtrlOrAltHeld(0));
    }
}
