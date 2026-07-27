using System.Text.Json;

namespace CatSit.Core.Settings;

public static class SettingsSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string ToJson(AppSettings settings) => JsonSerializer.Serialize(settings, Options);

    public static AppSettings FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new AppSettings();
        AppSettings? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
        if (parsed is null) return new AppSettings();
        parsed.Sensitivity = Math.Clamp(parsed.Sensitivity, 1, 10);
        parsed.SecondsUntilSleep = Math.Clamp(parsed.SecondsUntilSleep, 3, 60);
        if (parsed.ToggleHotkey is null || parsed.ToggleHotkey.Vk == 0)
        {
            parsed.ToggleHotkey = new AppSettings().ToggleHotkey;
        }
        return parsed;
    }
}
