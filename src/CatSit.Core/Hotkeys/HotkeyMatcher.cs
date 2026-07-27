using CatSit.Core.Settings;

namespace CatSit.Core.Hotkeys;

public static class HotkeyMatcher
{
    public static bool Matches(HotkeyCombo combo, int modifierState, int vk)
    {
        if (vk != combo.Vk) return false;
        if ((modifierState & ModifierTracker.RAlt) != 0) return false;
        var actual = HotkeyModifiers.None;
        if ((modifierState & (ModifierTracker.LCtrl | ModifierTracker.RCtrl)) != 0) actual |= HotkeyModifiers.Ctrl;
        if ((modifierState & ModifierTracker.LAlt) != 0) actual |= HotkeyModifiers.Alt;
        if ((modifierState & (ModifierTracker.LShift | ModifierTracker.RShift)) != 0) actual |= HotkeyModifiers.Shift;
        if ((modifierState & (ModifierTracker.LWin | ModifierTracker.RWin)) != 0) actual |= HotkeyModifiers.Win;
        return actual == combo.Modifiers;
    }
}
