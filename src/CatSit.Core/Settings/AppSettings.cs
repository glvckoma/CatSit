using CatSit.Core.Audio;
using CatSit.Core.Sprites;

namespace CatSit.Core.Settings;

public enum EntryEdge
{
    Random,
    Left,
    Right
}

public sealed class AppSettings
{
    public bool LockKeyboard { get; set; } = true;
    public bool LockMouse { get; set; }
    public bool DetectionEnabled { get; set; } = true;
    public int Sensitivity { get; set; } = 5;
    public int SecondsUntilSleep { get; set; } = 10;

    public bool WarnOnly { get; set; }
    public bool PauseInFullscreen { get; set; } = true;
    public bool ShowTrayAlerts { get; set; } = true;
    public bool ShowUnlockHint { get; set; } = true;
    public bool LearningEnabled { get; set; } = true;
    public int AutoUnlockMinutes { get; set; }
    public bool StartWithWindows { get; set; }
    public bool CloseToTray { get; set; } = true;
    public bool CheckForUpdatesAutomatically { get; set; } = true;
    public string? LastSkippedUpdateTag { get; set; }

    public bool SoundsEnabled { get; set; } = true;
    public int Volume { get; set; } = 70;
    public LockSound LockSound { get; set; } = LockSound.Chime;
    public CatSound CatSound { get; set; } = CatSound.Meow;
    public DeterrentSound DeterrentSound { get; set; } = DeterrentSound.None;
    public string? CustomLockSoundPath { get; set; }
    public string? CustomCatSoundPath { get; set; }
    public string? CustomDeterrentSoundPath { get; set; }

    public CatBreed Breed { get; set; } = CatBreed.OrangeTabby;
    public bool UseCustomFur { get; set; }
    public uint CustomFurColor { get; set; } = 0xFF6FD3B8;
    public int CatScale { get; set; } = 4;
    public EntryEdge EntryEdge { get; set; } = EntryEdge.Random;
    public bool ShowZzz { get; set; } = true;
    public bool ClickToDismiss { get; set; } = true;

    public List<string> ExcludedSignatures { get; set; } = [];

    public HotkeyCombo ToggleHotkey { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt,
        Vk = 0x4C
    };

    public CatTheme Theme() => UseCustomFur ? CatTheme.FromTint(CustomFurColor) : CatTheme.FromBreed(Breed);
}
