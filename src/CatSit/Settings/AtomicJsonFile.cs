using System.IO;
using System.Security;

namespace CatSit.Settings;

internal static class AtomicJsonFile
{
    public static PersistenceResult Write(string path, string json, string dataName)
    {
        string? temporaryPath = null;
        try
        {
            var directory = Path.GetDirectoryName(path) ?? string.Empty;
            Directory.CreateDirectory(directory);
            temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(path)) File.Replace(temporaryPath, path, null);
            else File.Move(temporaryPath, path);
            temporaryPath = null;
            return PersistenceResult.Ok();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            return PersistenceResult.Failed($"CatSit could not save {dataName}. Check access to your CatSit data folder and try again.");
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
                {
                }
            }
        }
    }
}
