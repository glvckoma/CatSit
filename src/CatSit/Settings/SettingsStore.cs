using System.IO;
using System.Security;
using CatSit.Core.Settings;

namespace CatSit.Settings;

public sealed class SettingsStore
{
    private readonly string _path;

    public SettingsStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CatSit",
            "settings.json");
    }

    public PersistenceLoadResult<AppSettings> Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new PersistenceLoadResult<AppSettings>(new AppSettings(), PersistenceResult.Ok());
            }

            var valid = SettingsSerializer.TryFromJson(File.ReadAllText(_path), out var settings);
            return valid
                ? new PersistenceLoadResult<AppSettings>(settings, PersistenceResult.Ok())
                : new PersistenceLoadResult<AppSettings>(settings, PersistenceResult.Failed(
                    "CatSit could not read your settings file, so default settings were loaded."));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            return new PersistenceLoadResult<AppSettings>(new AppSettings(), PersistenceResult.Failed(
                "CatSit could not read your settings file, so default settings were loaded."));
        }
    }

    public PersistenceResult Save(AppSettings settings)
    {
        try
        {
            return AtomicJsonFile.Write(_path, SettingsSerializer.ToJson(settings), "your settings");
        }
        catch (Exception exception) when (exception is System.Text.Json.JsonException
            or NotSupportedException or InvalidOperationException)
        {
            return PersistenceResult.Failed("CatSit could not save your settings. Try again or restart CatSit.");
        }
    }
}
