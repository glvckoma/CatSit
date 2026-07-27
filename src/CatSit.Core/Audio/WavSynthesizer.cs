namespace CatSit.Core.Audio;

public static class WavSynthesizer
{
    public const int SampleRate = 22050;
    private const double AmplitudeCap = 0.6;

    public static byte[] LockChime()
    {
        var beep1 = Beep(1400, 90);
        var gap = new double[MsToSamples(10)];
        var beep2 = Beep(1800, 90);
        return ToWav(Concat(beep1, gap, beep2));
    }

    private static double[] Beep(double frequency, int durationMs)
    {
        var samples = new double[MsToSamples(durationMs)];
        var attackSamples = MsToSamples(5);
        var decayTau = 0.040 * SampleRate;
        for (var i = 0; i < samples.Length; i++)
        {
            var envelope = i < attackSamples
                ? i / (double)attackSamples
                : Math.Exp(-(i - attackSamples) / decayTau);
            samples[i] = AmplitudeCap * envelope * Math.Sin(2 * Math.PI * frequency * i / SampleRate);
        }
        return samples;
    }

    internal static int MsToSamples(int ms) => SampleRate * ms / 1000;

    internal static double[] Concat(params double[][] segments)
    {
        var total = segments.Sum(s => s.Length);
        var result = new double[total];
        var offset = 0;
        foreach (var segment in segments)
        {
            segment.CopyTo(result, offset);
            offset += segment.Length;
        }
        return result;
    }

    internal static byte[] ToWav(double[] samples)
    {
        var dataSize = samples.Length * 2;
        using var stream = new MemoryStream(44 + dataSize);
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataSize);
        foreach (var sample in samples)
        {
            writer.Write((short)Math.Round(Math.Clamp(sample, -AmplitudeCap, AmplitudeCap) * short.MaxValue));
        }
        return stream.ToArray();
    }
}
