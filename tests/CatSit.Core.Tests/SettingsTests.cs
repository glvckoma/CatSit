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
}
