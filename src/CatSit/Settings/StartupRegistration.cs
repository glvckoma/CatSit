using System.IO;
using System.Security;
using Microsoft.Win32;

namespace CatSit.Settings;

public static class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "CatSit";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string value && value.Length > 0;
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }

    public static string BuildCommand(string executablePath) => $"\"{executablePath}\" --background";

    public static void Apply(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (key is null) return;
            if (enabled)
            {
                var path = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(path)) return;
                key.SetValue(ValueName, BuildCommand(path));
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException or IOException)
        {
        }
    }

    public static void Sync(bool desired)
    {
        if (!desired)
        {
            if (IsEnabled()) Apply(false);
            return;
        }

        var path = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            if (!string.Equals(key?.GetValue(ValueName) as string, BuildCommand(path), StringComparison.OrdinalIgnoreCase))
            {
                Apply(true);
            }
        }
        catch (Exception exception) when (exception is SecurityException or UnauthorizedAccessException or IOException)
        {
        }
    }
}
