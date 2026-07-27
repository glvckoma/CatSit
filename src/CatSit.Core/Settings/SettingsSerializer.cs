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
        if (string.IsNullOrWhiteSpace(json)) return new AppSettings();
        AppSettings? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AppSettings>(json, Options);
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
        if (parsed is null) return new AppSettings();
        parsed.Sensitivity = Math.Clamp(parsed.Sensitivity, 1, 10);
        parsed.SecondsUntilSleep = Math.Clamp(parsed.SecondsUntilSleep, 3, 60);
        parsed.Volume = Math.Clamp(parsed.Volume, 0, 100);
        parsed.CatScale = Math.Clamp(parsed.CatScale, 2, 8);
        parsed.AutoUnlockMinutes = Math.Clamp(parsed.AutoUnlockMinutes, 0, 60);
        parsed.ExcludedSignatures ??= [];
        if (!parsed.LockKeyboard && !parsed.LockMouse) parsed.LockKeyboard = true;
        if (parsed.ToggleHotkey is null || parsed.ToggleHotkey.Vk == 0)
        {
            parsed.ToggleHotkey = new AppSettings().ToggleHotkey;
        }
        return parsed;
    }
}
