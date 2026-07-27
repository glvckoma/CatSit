namespace CatSit.Core.Hotkeys;

public static class ModifierTracker
{
    public const int LCtrl = 1;
    public const int RCtrl = 2;
    public const int LAlt = 4;
    public const int RAlt = 8;
    public const int LShift = 16;
    public const int RShift = 32;
    public const int LWin = 64;
    public const int RWin = 128;

    public static int Update(int state, int vk, bool isDown)
    {
        var bit = vk switch
        {
            0xA2 => LCtrl,
            0xA3 => RCtrl,
            0xA4 => LAlt,
            0xA5 => RAlt,
            0xA0 => LShift,
            0xA1 => RShift,
            0x5B => LWin,
            0x5C => RWin,
            _ => 0
        };
        if (bit == 0) return state;
        return isDown ? state | bit : state & ~bit;
    }

    public static bool CtrlOrAltHeld(int state) => (state & (LCtrl | RCtrl | LAlt | RAlt)) != 0;
}
