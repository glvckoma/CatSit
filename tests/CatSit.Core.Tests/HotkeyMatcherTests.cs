using CatSit.Core.Hotkeys;
using CatSit.Core.Settings;

namespace CatSit.Core.Tests;

public class HotkeyMatcherTests
{
    private static readonly HotkeyCombo CtrlAltL = new()
    {
        Modifiers = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt,
        Vk = 0x4C
    };

    [Fact]
    public void Matches_LeftCtrlLeftAlt_True()
    {
        Assert.True(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl | ModifierTracker.LAlt, 0x4C));
    }

    [Fact]
    public void Matches_RightCtrlLeftAlt_True()
    {
        Assert.True(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.RCtrl | ModifierTracker.LAlt, 0x4C));
    }

    [Fact]
    public void Matches_AltGrSignature_False()
    {
        Assert.False(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl | ModifierTracker.RAlt, 0x4C));
    }

    [Fact]
    public void Matches_RightAltOnlyAsAlt_False()
    {
        Assert.False(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl | ModifierTracker.LAlt | ModifierTracker.RAlt, 0x4C));
    }

    [Fact]
    public void Matches_ExtraModifier_False()
    {
        Assert.False(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl | ModifierTracker.LAlt | ModifierTracker.LShift, 0x4C));
    }

    [Fact]
    public void Matches_MissingModifier_False()
    {
        Assert.False(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl, 0x4C));
    }

    [Fact]
    public void Matches_WrongKey_False()
    {
        Assert.False(HotkeyMatcher.Matches(CtrlAltL, ModifierTracker.LCtrl | ModifierTracker.LAlt, 0x4B));
    }

    [Fact]
    public void Matches_ShiftF5_EitherShift()
    {
        var combo = new HotkeyCombo { Modifiers = HotkeyModifiers.Shift, Vk = 0x74 };
        Assert.True(HotkeyMatcher.Matches(combo, ModifierTracker.LShift, 0x74));
        Assert.True(HotkeyMatcher.Matches(combo, ModifierTracker.RShift, 0x74));
    }

    [Fact]
    public void Matches_WinCombo()
    {
        var combo = new HotkeyCombo { Modifiers = HotkeyModifiers.Win, Vk = 0x43 };
        Assert.True(HotkeyMatcher.Matches(combo, ModifierTracker.LWin, 0x43));
        Assert.False(HotkeyMatcher.Matches(combo, 0, 0x43));
    }
}
