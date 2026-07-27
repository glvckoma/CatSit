namespace CatSit.Core.Detection;

public sealed class ExclusionList
{
    private readonly HashSet<string> _signatures = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> Signatures => _signatures;

    public void Add(string signature)
    {
        if (!string.IsNullOrWhiteSpace(signature)) _signatures.Add(signature);
    }

    public void Remove(string signature) => _signatures.Remove(signature);

    public bool IsExcluded(CatSignal signal) => _signatures.Contains(signal.Signature);

    public void Load(IEnumerable<string> signatures)
    {
        _signatures.Clear();
        foreach (var signature in signatures) Add(signature);
    }
}
