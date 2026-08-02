using CatSit.Core.Detection;

namespace CatSit.Core.Learning;

public sealed record LabeledSample
{
    public string Id { get; init; } = string.Empty;
    public double[] Features { get; init; } = [];
    public bool IsCat { get; init; }
}

public sealed class CatClassifier
{
    public const int FeatureCount = 8;
    public const int MinSamplesBeforeVeto = 6;
    public const int MinSamplesPerClass = 2;
    public const int MaxSamples = 60;

    private const double SuppressBelowScore = 0.35;
    private const double LearningRate = 0.35;
    private const int TrainingEpochs = 12;

    private static readonly double[] InitialWeights = [1.2, 1.0, 1.0, 0.8, 1.0, 0.8, 0.8, 0.6];
    private const double InitialBias = -1.0;

    private readonly List<LabeledSample> _samples = [];
    private double[] _weights = [.. InitialWeights];
    private double _bias = InitialBias;
    private int _generatedId;

    public IReadOnlyList<double> Weights => _weights;
    public double Bias => _bias;
    public int SampleCount => _samples.Count;
    public int CatSampleCount => _samples.Count(sample => sample.IsCat);
    public int FalseAlarmSampleCount => _samples.Count(sample => !sample.IsCat);
    public IReadOnlyList<LabeledSample> Samples => _samples;
    public bool IsActive => SampleCount >= MinSamplesBeforeVeto
        && CatSampleCount >= MinSamplesPerClass
        && FalseAlarmSampleCount >= MinSamplesPerClass;

    public double Score(DetectionFeatures features) => Sigmoid(Logit(features.ToVector()));

    public bool ShouldSuppress(DetectionFeatures features) =>
        IsActive && Score(features) < SuppressBelowScore;

    public void Learn(DetectionFeatures features, bool isCat)
    {
        string id;
        do
        {
            id = $"generated-{_generatedId++}";
        }
        while (_samples.Any(sample => sample.Id == id));
        SetLabel(id, features, isCat);
    }

    public void SetLabel(string id, DetectionFeatures features, bool isCat)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A sample ID is required.", nameof(id));
        var sample = new LabeledSample { Id = id, Features = features.ToVector(), IsCat = isCat };
        var index = _samples.FindIndex(existing => existing.Id == id);
        if (index >= 0) _samples[index] = sample;
        else _samples.Add(sample);
        TrimSamples();
        Train();
    }

    public bool RemoveLabel(string id)
    {
        var removed = _samples.RemoveAll(sample => sample.Id == id) > 0;
        if (removed) Train();
        return removed;
    }

    public void LoadSamples(IEnumerable<LabeledSample>? samples)
    {
        Reset();
        if (samples is null) return;

        var generatedIndex = 0;
        var valid = new List<LabeledSample>();
        foreach (var sample in samples)
        {
            if (sample is null || !IsValidVector(sample.Features)) continue;
            var id = string.IsNullOrWhiteSpace(sample.Id) ? $"legacy-{generatedIndex}" : sample.Id;
            generatedIndex++;
            valid.RemoveAll(existing => existing.Id == id);
            valid.Add(sample with { Id = id, Features = [.. sample.Features] });
        }

        _samples.Clear();
        _samples.AddRange(valid.TakeLast(MaxSamples));
        _generatedId = generatedIndex;
        Train();
    }

    public void Reset()
    {
        _weights = [.. InitialWeights];
        _bias = InitialBias;
        _samples.Clear();
        _generatedId = 0;
    }

    private void TrimSamples()
    {
        if (_samples.Count > MaxSamples) _samples.RemoveRange(0, _samples.Count - MaxSamples);
    }

    private void Train()
    {
        _weights = [.. InitialWeights];
        _bias = InitialBias;
        for (var epoch = 0; epoch < TrainingEpochs; epoch++)
        {
            foreach (var sample in _samples)
            {
                var prediction = Sigmoid(Logit(sample.Features));
                var error = (sample.IsCat ? 1.0 : 0.0) - prediction;
                for (var i = 0; i < FeatureCount; i++)
                {
                    _weights[i] += LearningRate * error * sample.Features[i];
                }
                _bias += LearningRate * error;
            }
        }
    }

    private double Logit(double[] vector)
    {
        var sum = _bias;
        for (var i = 0; i < FeatureCount; i++) sum += _weights[i] * vector[i];
        return sum;
    }

    private static bool IsValidVector(double[]? vector) => vector is { Length: FeatureCount }
        && vector.All(value => double.IsFinite(value) && value is >= 0 and <= 1);

    private static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
}
