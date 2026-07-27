namespace CatSit.Core.Audio;

public static class WavSynthesizer
{
    public const int SampleRate = 22050;
    private const double AmplitudeCap = 0.6;

    public static byte[] For(LockSound sound) => sound switch
    {
        LockSound.Chime => LockChime(),
        LockSound.Beep => Beep(),
        LockSound.Thud => Thud(),
        LockSound.Alarm => Alarm(),
        _ => []
    };

    public static byte[] For(CatSound sound) => sound switch
    {
        CatSound.Meow => Meow(),
        CatSound.Chirp => Chirp(),
        CatSound.Trill => Trill(),
        _ => []
    };

    public static byte[] For(DeterrentSound sound) => sound switch
    {
        DeterrentSound.Hiss => Hiss(),
        DeterrentSound.Shake => Shake(),
        _ => []
    };

    public static byte[] Beep() => ToWav(Tone(1000, 140, 0.005, 0.070));

    public static byte[] Thud()
    {
        var samples = new double[MsToSamples(220)];
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i / (double)SampleRate;
            var frequency = 150 - 40 * (t / 0.220);
            var envelope = Math.Exp(-t / 0.055);
            samples[i] = AmplitudeCap * envelope * Math.Sin(2 * Math.PI * frequency * t);
        }
        return ToWav(samples);
    }

    public static byte[] Alarm() => ToWav(Concat(
        Tone(880, 110, 0.004, 0.045),
        new double[MsToSamples(40)],
        Tone(1170, 110, 0.004, 0.045),
        new double[MsToSamples(40)],
        Tone(1480, 150, 0.004, 0.060)));

    public static byte[] Chirp()
    {
        var totalSamples = MsToSamples(200);
        var samples = new double[totalSamples];
        var phase = 0.0;
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            var frequency = 700 + 700 * (t / 0.200);
            phase += 2 * Math.PI * frequency / SampleRate;
            var envelope = Math.Min(1, t / 0.015) * Math.Exp(-t / 0.090);
            samples[i] = AmplitudeCap * envelope * (0.8 * Math.Sin(phase) + 0.2 * Math.Sin(2 * phase));
        }
        return ToWav(samples);
    }

    public static byte[] Trill()
    {
        var totalSamples = MsToSamples(450);
        var samples = new double[totalSamples];
        var phase = 0.0;
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            var frequency = 560 + 60 * Math.Sin(2 * Math.PI * 5 * t);
            phase += 2 * Math.PI * frequency / SampleRate;
            var tremolo = 0.55 + 0.45 * Math.Sin(2 * Math.PI * 26 * t);
            var envelope = Math.Min(1, t / 0.030) * Math.Min(1, (0.450 - t) / 0.090);
            samples[i] = AmplitudeCap * envelope * tremolo * (0.7 * Math.Sin(phase) + 0.3 * Math.Sin(2 * phase));
        }
        return ToWav(samples);
    }

    public static byte[] Hiss()
    {
        var totalSamples = MsToSamples(550);
        var samples = new double[totalSamples];
        var noise = new DeterministicNoise(20260727);
        var previous = 0.0;
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            var white = noise.Next();
            var highPassed = white - previous;
            previous = white;
            var envelope = Math.Min(1, t / 0.040) * Math.Min(1, (0.550 - t) / 0.180);
            samples[i] = AmplitudeCap * envelope * highPassed * 0.75;
        }
        return ToWav(samples);
    }

    public static byte[] Shake()
    {
        var totalSamples = MsToSamples(600);
        var samples = new double[totalSamples];
        var noise = new DeterministicNoise(864213);
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            var rattlePhase = t % 0.100;
            var burst = Math.Exp(-rattlePhase / 0.018);
            var envelope = Math.Min(1, t / 0.020) * Math.Min(1, (0.600 - t) / 0.150);
            samples[i] = AmplitudeCap * envelope * burst * noise.Next();
        }
        return ToWav(samples);
    }

    private static double[] Tone(double frequency, int durationMs, double attackSeconds, double decayTau)
    {
        var samples = new double[MsToSamples(durationMs)];
        var attackSamples = Math.Max(1, (int)(attackSeconds * SampleRate));
        for (var i = 0; i < samples.Length; i++)
        {
            var envelope = i < attackSamples
                ? i / (double)attackSamples
                : Math.Exp(-(i - attackSamples) / (decayTau * SampleRate));
            samples[i] = AmplitudeCap * envelope * Math.Sin(2 * Math.PI * frequency * i / SampleRate);
        }
        return samples;
    }

    private sealed class DeterministicNoise
    {
        private uint _state;

        public DeterministicNoise(uint seed)
        {
            _state = seed == 0 ? 1 : seed;
        }

        public double Next()
        {
            _state = _state * 1664525 + 1013904223;
            return (_state >> 8) / (double)(1 << 23) - 1.0;
        }
    }

    public static byte[] Meow()
    {
        var totalSamples = MsToSamples(600);
        var samples = new double[totalSamples];
        var phase = 0.0;
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            phase += 2 * Math.PI * MeowFrequency(t) / SampleRate;
            var wave = 0.6 * Math.Sin(phase) + 0.25 * Math.Sin(2 * phase) + 0.10 * Math.Sin(3 * phase) + 0.05 * Math.Sin(4 * phase);
            samples[i] = AmplitudeCap * MeowEnvelope(t) * wave;
        }
        return ToWav(samples);
    }

    private static double MeowFrequency(double t)
    {
        if (t < 0.150) return 500 + 300 * (t / 0.150);
        if (t < 0.350) return 800 * (1 + 0.03 * Math.Sin(2 * Math.PI * 6 * t));
        return 800 - 400 * ((t - 0.350) / 0.250);
    }

    private static double MeowEnvelope(double t)
    {
        const double attack = 0.030;
        const double release = 0.150;
        const double total = 0.600;
        if (t < attack) return t / attack;
        if (t > total - release) return Math.Max(0, (total - t) / release);
        return 1.0;
    }

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
