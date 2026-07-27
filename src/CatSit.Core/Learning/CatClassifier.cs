using CatSit.Core.Detection;

namespace CatSit.Core.Learning;

public sealed record LabeledSample(double[] Features, bool IsCat);

public sealed class CatClassifier
{
    public const int FeatureCount = 8;
    public const int MinSamplesBeforeVeto = 3;
    public const int MaxSamples = 60;

    private const double SuppressBelowScore = 0.35;
    private const double LearningRate = 0.35;
    private const int TrainingEpochs = 12;

    private static readonly double[] InitialWeights = [1.2, 1.0, 1.0, 0.8, 1.0, 0.8, 0.8, 0.6];
    private const double InitialBias = -1.0;

    private readonly List<LabeledSample> _samples = [];
    private double[] _weights = [.. InitialWeights];
    private double _bias = InitialBias;

    public IReadOnlyList<double> Weights => _weights;
    public double Bias => _bias;
    public int SampleCount => _samples.Count;
    public IReadOnlyList<LabeledSample> Samples => _samples;
    public bool IsActive => _samples.Count >= MinSamplesBeforeVeto;

    public double Score(DetectionFeatures features) => Sigmoid(Logit(features.ToVector()));

    public bool ShouldSuppress(DetectionFeatures features) =>
        IsActive && Score(features) < SuppressBelowScore;

    public void Learn(DetectionFeatures features, bool isCat)
    {
        _samples.Add(new LabeledSample(features.ToVector(), isCat));
        if (_samples.Count > MaxSamples) _samples.RemoveRange(0, _samples.Count - MaxSamples);
        Train();
    }

    public void Load(IEnumerable<double> weights, double bias, IEnumerable<LabeledSample> samples)
    {
        var loaded = weights.ToArray();
        if (loaded.Length == FeatureCount)
        {
            _weights = loaded;
            _bias = bias;
        }
        _samples.Clear();
        _samples.AddRange(samples.Where(sample => sample.Features.Length == FeatureCount).TakeLast(MaxSamples));
    }

    public void Reset()
    {
        _weights = [.. InitialWeights];
        _bias = InitialBias;
        _samples.Clear();
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

    private static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
}
