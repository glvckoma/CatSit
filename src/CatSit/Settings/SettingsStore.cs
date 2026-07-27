using System.IO;
using CatSit.Core.Settings;

namespace CatSit.Settings;

public sealed class SettingsStore
{
    private readonly string _path;

    public SettingsStore()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CatSit");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            return SettingsSerializer.FromJson(File.Exists(_path) ? File.ReadAllText(_path) : null);
        }
        catch (IOException)
        {
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            File.WriteAllText(_path, SettingsSerializer.ToJson(settings));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
