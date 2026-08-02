using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatSit.Core.Settings;

public static class SettingsSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string ToJson(AppSettings settings) => JsonSerializer.Serialize(settings, Options);

    public static AppSettings FromJson(string? json)
    {
        TryFromJson(json, out var settings);
        return settings;
    }

    public static bool TryFromJson(string? json, out AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            settings = new AppSettings();
            return true;
        }

        AppSettings? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AppSettings>(json, Options);
        }
        catch (JsonException)
        {
            settings = new AppSettings();
            return false;
        }

        if (parsed is null)
        {
            settings = new AppSettings();
            return false;
        }

        parsed.Sensitivity = Math.Clamp(parsed.Sensitivity, 1, 10);
        parsed.SecondsUntilSleep = Math.Clamp(parsed.SecondsUntilSleep, 3, 60);
        parsed.Volume = Math.Clamp(parsed.Volume, 0, 100);
        parsed.CatScale = Math.Clamp(parsed.CatScale, 2, 8);
        parsed.AutoUnlockMinutes = Math.Clamp(parsed.AutoUnlockMinutes, 0, 60);
        parsed.ExcludedSignatures ??= [];
        if (!parsed.LockKeyboard && !parsed.LockMouse) parsed.LockKeyboard = true;
        if (parsed.DetectionEnabled && !parsed.DetectKeyboardInput && !parsed.DetectMouseInput)
        {
            parsed.DetectKeyboardInput = true;
        }
        if (parsed.ToggleHotkey is null || parsed.ToggleHotkey.Vk == 0)
        {
            parsed.ToggleHotkey = new AppSettings().ToggleHotkey;
        }
        settings = parsed;
        return true;
    }
}
