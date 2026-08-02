using System.IO;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatSit.Core.Learning;

namespace CatSit.Settings;

public sealed record ActivityState
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public List<TriggerRecord> Triggers { get; init; } = [];
    public List<LabeledSample> Samples { get; init; } = [];
}

public sealed class ActivityStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public ActivityStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CatSit",
            "activity.json");
    }

    public PersistenceLoadResult<ActivityState> Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new PersistenceLoadResult<ActivityState>(new ActivityState(), PersistenceResult.Ok());
            }

            var file = JsonSerializer.Deserialize<ActivityFile>(File.ReadAllText(_path), Options);
            if (file is null)
            {
                return FailedLoad();
            }

            var triggers = (file.Triggers ?? [])
                .Where(trigger => trigger is not null)
                .Select(trigger => trigger with { Features = trigger.Features ?? new() })
                .ToList();

            if (file.SchemaVersion > ActivityState.CurrentSchemaVersion)
            {
                var preserved = new ActivityState { Triggers = triggers };
                return new PersistenceLoadResult<ActivityState>(preserved, PersistenceResult.Failed(
                    "CatSit kept your activity history but could not load learning data from a newer format."));
            }

            var samples = file.SchemaVersion < ActivityState.CurrentSchemaVersion
                ? MigrateLegacySamples(triggers, file.Samples ?? [])
                : NormalizeSamples(file.Samples ?? []);
            var state = new ActivityState { Triggers = triggers, Samples = samples };
            return new PersistenceLoadResult<ActivityState>(state, PersistenceResult.Ok());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or SecurityException or JsonException or NotSupportedException)
        {
            return FailedLoad();
        }
    }

    public PersistenceResult Save(TriggerLog log, CatClassifier classifier)
    {
        try
        {
            var state = new ActivityState
            {
                Triggers = [.. log.Entries],
                Samples = [.. classifier.Samples]
            };
            return AtomicJsonFile.Write(
                _path,
                JsonSerializer.Serialize(state, Options),
                "your activity and learning data");
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException or InvalidOperationException)
        {
            return PersistenceResult.Failed(
                "CatSit could not save your activity and learning data. Try again or restart CatSit.");
        }
    }

    private static PersistenceLoadResult<ActivityState> FailedLoad() =>
        new(new ActivityState(), PersistenceResult.Failed(
            "CatSit could not read your activity and learning data, so a fresh history was loaded."));

    private static List<LabeledSample> MigrateLegacySamples(
        IReadOnlyList<TriggerRecord> triggers,
        IEnumerable<LabeledSample> samples)
    {
        var usedTriggerIds = new HashSet<string>(StringComparer.Ordinal);
        var migrated = new List<LabeledSample>();
        var index = 0;
        foreach (var sample in samples)
        {
            if (!IsValidSample(sample)) continue;
            var id = sample.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                var match = triggers.FirstOrDefault(trigger =>
                    !usedTriggerIds.Contains(trigger.Id)
                    && trigger.Verdict != TriggerVerdict.Unreviewed
                    && (trigger.Verdict == TriggerVerdict.ConfirmedCat) == sample.IsCat
                    && VectorsMatch(trigger.Features.ToVector(), sample.Features));
                id = match?.Id ?? $"legacy-{index:D3}";
                if (match is not null) usedTriggerIds.Add(match.Id);
            }

            migrated.RemoveAll(existing => existing.Id == id);
            migrated.Add(sample with { Id = id, Features = [.. sample.Features] });
            index++;
        }
        return [.. migrated.TakeLast(CatClassifier.MaxSamples)];
    }

    private static List<LabeledSample> NormalizeSamples(IEnumerable<LabeledSample> samples)
    {
        var normalized = new List<LabeledSample>();
        var index = 0;
        foreach (var sample in samples)
        {
            if (!IsValidSample(sample)) continue;
            var id = string.IsNullOrWhiteSpace(sample.Id) ? $"legacy-{index:D3}" : sample.Id;
            normalized.RemoveAll(existing => existing.Id == id);
            normalized.Add(sample with { Id = id, Features = [.. sample.Features] });
            index++;
        }
        return [.. normalized.TakeLast(CatClassifier.MaxSamples)];
    }

    private static bool IsValidSample(LabeledSample? sample) => sample?.Features is { Length: CatClassifier.FeatureCount }
        && sample.Features.All(value => double.IsFinite(value) && value is >= 0 and <= 1);

    private static bool VectorsMatch(double[] left, double[] right) =>
        left.Length == right.Length && left.Zip(right).All(pair => Math.Abs(pair.First - pair.Second) < 0.000000001);

    private sealed record ActivityFile
    {
        public int SchemaVersion { get; init; }
        public List<TriggerRecord>? Triggers { get; init; }
        public List<LabeledSample>? Samples { get; init; }
        public List<double>? Weights { get; init; }
        public double Bias { get; init; }
    }
}
