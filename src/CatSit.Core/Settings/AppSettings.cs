namespace CatSit.Core.Settings;

public sealed class AppSettings
{
    public bool LockKeyboard { get; set; } = true;
    public bool LockMouse { get; set; }
    public bool DetectionEnabled { get; set; } = true;
    public int Sensitivity { get; set; } = 5;
    public int SecondsUntilSleep { get; set; } = 10;
    public bool SoundsEnabled { get; set; } = true;

    public HotkeyCombo ToggleHotkey { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt,
        Vk = 0x4C
    };
}
