using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatSit.Core.Learning;

namespace CatSit.Settings;

public sealed record ActivityState(
    List<TriggerRecord> Triggers,
    List<double> Weights,
    double Bias,
    List<LabeledSample> Samples);

public sealed class ActivityStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public ActivityStore()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CatSit");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "activity.json");
    }

    public ActivityState Load()
    {
        try
        {
            if (!File.Exists(_path)) return Empty();
            return JsonSerializer.Deserialize<ActivityState>(File.ReadAllText(_path), Options) ?? Empty();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            return Empty();
        }
    }

    public void Save(TriggerLog log, CatClassifier classifier)
    {
        try
        {
            var state = new ActivityState(
                [.. log.Entries],
                [.. classifier.Weights],
                classifier.Bias,
                [.. classifier.Samples]);
            File.WriteAllText(_path, JsonSerializer.Serialize(state, Options));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static ActivityState Empty() => new([], [], 0, []);
}
