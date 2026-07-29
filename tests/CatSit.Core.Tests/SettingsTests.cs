using CatSit.Core.Settings;

namespace CatSit.Core.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Defaults_MatchSpec()
    {
        var s = new AppSettings();
        Assert.True(s.LockKeyboard);
        Assert.False(s.LockMouse);
        Assert.True(s.DetectionEnabled);
        Assert.Equal(5, s.Sensitivity);
        Assert.Equal(10, s.SecondsUntilSleep);
        Assert.True(s.SoundsEnabled);
        Assert.Equal(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, s.ToggleHotkey.Modifiers);
        Assert.Equal(0x4C, s.ToggleHotkey.Vk);
    }
}

public class HotkeyComboTests
{
    [Fact]
    public void ToString_FormatsCtrlAltL()
    {
        var combo = new HotkeyCombo { Modifiers = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, Vk = 0x4C };
        Assert.Equal("Ctrl+Alt+L", combo.ToString());
    }

    [Fact]
    public void ToString_FormatsShiftF5()
    {
        var combo = new HotkeyCombo { Modifiers = HotkeyModifiers.Shift, Vk = 0x74 };
        Assert.Equal("Shift+F5", combo.ToString());
    }

    [Fact]
    public void ToString_FormatsDigitAndSpace()
    {
        Assert.Equal("Ctrl+7", new HotkeyCombo { Modifiers = HotkeyModifiers.Ctrl, Vk = 0x37 }.ToString());
        Assert.Equal("Win+Space", new HotkeyCombo { Modifiers = HotkeyModifiers.Win, Vk = 0x20 }.ToString());
    }
}

public class SettingsSerializerTests
{
    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var s = new AppSettings
        {
            LockKeyboard = false,
            LockMouse = true,
            DetectionEnabled = false,
            Sensitivity = 8,
            SecondsUntilSleep = 42,
            SoundsEnabled = false,
            ToggleHotkey = new HotkeyCombo { Modifiers = HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, Vk = 0x55 }
        };
        var restored = SettingsSerializer.FromJson(SettingsSerializer.ToJson(s));
        Assert.False(restored.LockKeyboard);
        Assert.True(restored.LockMouse);
        Assert.False(restored.DetectionEnabled);
        Assert.Equal(8, restored.Sensitivity);
        Assert.Equal(42, restored.SecondsUntilSleep);
        Assert.False(restored.SoundsEnabled);
        Assert.Equal(HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, restored.ToggleHotkey.Modifiers);
        Assert.Equal(0x55, restored.ToggleHotkey.Vk);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json at all")]
    [InlineData("{\"LockKeyboard\":")]
    public void FromJson_InvalidInput_ReturnsDefaults(string? json)
    {
        var restored = SettingsSerializer.FromJson(json);
        Assert.True(restored.LockKeyboard);
        Assert.Equal(5, restored.Sensitivity);
        Assert.Equal(0x4C, restored.ToggleHotkey.Vk);
    }

    [Fact]
    public void FromJson_OutOfRangeValues_AreClamped()
    {
        var restored = SettingsSerializer.FromJson("{\"Sensitivity\":99,\"SecondsUntilSleep\":0}");
        Assert.Equal(10, restored.Sensitivity);
        Assert.Equal(3, restored.SecondsUntilSleep);
    }

    [Fact]
    public void FromJson_MissingHotkey_UsesDefault()
    {
        var restored = SettingsSerializer.FromJson("{\"LockMouse\":true}");
        Assert.True(restored.LockMouse);
        Assert.Equal(0x4C, restored.ToggleHotkey.Vk);
        Assert.Equal(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, restored.ToggleHotkey.Modifiers);
    }

    [Fact]
    public void RoundTrip_PreservesV2Options()
    {
        var settings = new AppSettings
        {
            WarnOnly = true,
            PauseInFullscreen = false,
            ShowTrayAlerts = false,
            LearningEnabled = false,
            AutoUnlockMinutes = 15,
            Volume = 40,
            LockSound = CatSit.Core.Audio.LockSound.Thud,
            CatSound = CatSit.Core.Audio.CatSound.Trill,
            DeterrentSound = CatSit.Core.Audio.DeterrentSound.Hiss,
            CustomCatSoundPath = @"C:\sounds\mycat.wav",
            Breed = CatSit.Core.Sprites.CatBreed.Tuxedo,
            UseCustomFur = true,
            CustomFurColor = 0xFF123456,
            CatScale = 6,
            EntryEdge = EntryEdge.Left,
            ShowZzz = false,
            ClickToDismiss = false,
            ExcludedSignatures = ["W+D", "A+S"]
        };
        var restored = SettingsSerializer.FromJson(SettingsSerializer.ToJson(settings));
        Assert.True(restored.WarnOnly);
        Assert.False(restored.PauseInFullscreen);
        Assert.False(restored.ShowTrayAlerts);
        Assert.False(restored.LearningEnabled);
        Assert.Equal(15, restored.AutoUnlockMinutes);
        Assert.Equal(40, restored.Volume);
        Assert.Equal(CatSit.Core.Audio.LockSound.Thud, restored.LockSound);
        Assert.Equal(CatSit.Core.Audio.CatSound.Trill, restored.CatSound);
        Assert.Equal(CatSit.Core.Audio.DeterrentSound.Hiss, restored.DeterrentSound);
        Assert.Equal(@"C:\sounds\mycat.wav", restored.CustomCatSoundPath);
        Assert.Equal(CatSit.Core.Sprites.CatBreed.Tuxedo, restored.Breed);
        Assert.True(restored.UseCustomFur);
        Assert.Equal(0xFF123456u, restored.CustomFurColor);
        Assert.Equal(6, restored.CatScale);
        Assert.Equal(EntryEdge.Left, restored.EntryEdge);
        Assert.False(restored.ShowZzz);
        Assert.False(restored.ClickToDismiss);
        Assert.Equal(2, restored.ExcludedSignatures.Count);
    }

    [Fact]
    public void FromJson_ClampsV2RangesAndKeepsAtLeastOneLockTarget()
    {
        var restored = SettingsSerializer.FromJson(
            "{\"Volume\":900,\"CatScale\":99,\"AutoUnlockMinutes\":-4,\"LockKeyboard\":false,\"LockMouse\":false}");
        Assert.Equal(100, restored.Volume);
        Assert.Equal(8, restored.CatScale);
        Assert.Equal(0, restored.AutoUnlockMinutes);
        Assert.True(restored.LockKeyboard);
    }

    [Fact]
    public void FromJson_V1FileStillLoadsWithNewDefaults()
    {
        var restored = SettingsSerializer.FromJson(
            "{\"LockKeyboard\":true,\"Sensitivity\":7,\"SecondsUntilSleep\":20,\"SoundsEnabled\":true}");
        Assert.Equal(7, restored.Sensitivity);
        Assert.Equal(20, restored.SecondsUntilSleep);
        Assert.Equal(CatSit.Core.Sprites.CatBreed.OrangeTabby, restored.Breed);
        Assert.True(restored.PauseInFullscreen);
        Assert.Equal(70, restored.Volume);
        Assert.Empty(restored.ExcludedSignatures);
    }

    [Fact]
    public void WindowsIntegrationDefaults_AreOffForStartupAndOnForTray()
    {
        var settings = new AppSettings();
        Assert.False(settings.StartWithWindows);
        Assert.True(settings.CloseToTray);
    }

    [Fact]
    public void RoundTrip_PreservesWindowsIntegrationOptions()
    {
        var restored = SettingsSerializer.FromJson(SettingsSerializer.ToJson(
            new AppSettings { StartWithWindows = true, CloseToTray = false }));
        Assert.True(restored.StartWithWindows);
        Assert.False(restored.CloseToTray);
    }

    [Fact]
    public void RoundTrip_PreservesUpdateCheckOptions()
    {
        var restored = SettingsSerializer.FromJson(SettingsSerializer.ToJson(
            new AppSettings { CheckForUpdatesAutomatically = false, LastSkippedUpdateTag = "v0.2.0" }));
        Assert.False(restored.CheckForUpdatesAutomatically);
        Assert.Equal("v0.2.0", restored.LastSkippedUpdateTag);
    }

    [Fact]
    public void Defaults_EnableUpdateChecksWithNoSkippedVersion()
    {
        var settings = new AppSettings();
        Assert.True(settings.CheckForUpdatesAutomatically);
        Assert.Null(settings.LastSkippedUpdateTag);
    }

    [Fact]
    public void Theme_FollowsBreedOrCustomFur()
    {
        var settings = new AppSettings { Breed = CatSit.Core.Sprites.CatBreed.Black };
        Assert.Equal(CatSit.Core.Sprites.CatTheme.FromBreed(CatSit.Core.Sprites.CatBreed.Black), settings.Theme());
        settings.UseCustomFur = true;
        settings.CustomFurColor = 0xFF4C8BF5;
        Assert.Equal(0xFF4C8BF5u, settings.Theme().Fur);
    }
}
