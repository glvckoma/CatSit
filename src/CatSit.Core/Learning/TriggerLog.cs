using CatSit.Core.Detection;

namespace CatSit.Core.Learning;

public sealed class TriggerLog
{
    public const int MaxEntries = 50;

    private readonly List<TriggerRecord> _entries = [];

    public IReadOnlyList<TriggerRecord> Entries => _entries;

    public TriggerRecord Add(
        CatSignal signal,
        bool locked,
        DateTimeOffset timestampUtc,
        string id,
        bool suppressedByLearning = false)
    {
        var record = new TriggerRecord
        {
            Id = id,
            TimestampUtc = timestampUtc,
            Rule = signal.Rule,
            Signature = signal.Signature,
            Locked = locked,
            SuppressedByLearning = suppressedByLearning,
            Features = signal.Features
        };
        _entries.Insert(0, record);
        if (_entries.Count > MaxEntries) _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
        return record;
    }

    public TriggerRecord? SetVerdict(string id, TriggerVerdict verdict)
    {
        var index = _entries.FindIndex(record => record.Id == id);
        if (index < 0) return null;
        var updated = _entries[index] with { Verdict = verdict };
        _entries[index] = updated;
        return updated;
    }

    public void Load(IEnumerable<TriggerRecord> records)
    {
        _entries.Clear();
        _entries.AddRange(records.OrderByDescending(record => record.TimestampUtc).Take(MaxEntries));
    }

    public void Clear() => _entries.Clear();

    public void ResetVerdicts()
    {
        for (var i = 0; i < _entries.Count; i++)
        {
            _entries[i] = _entries[i] with { Verdict = TriggerVerdict.Unreviewed };
        }
    }
}
