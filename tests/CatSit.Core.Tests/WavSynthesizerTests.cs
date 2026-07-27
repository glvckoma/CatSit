using CatSit.Core.Audio;

namespace CatSit.Core.Tests;

public class WavSynthesizerTests
{
    [Fact]
    public void LockChime_HasValidRiffWaveHeader()
    {
        var wav = WavSynthesizer.LockChime();
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        Assert.Equal("fmt ", System.Text.Encoding.ASCII.GetString(wav, 12, 4));
        Assert.Equal("data", System.Text.Encoding.ASCII.GetString(wav, 36, 4));
        Assert.Equal(wav.Length - 8, BitConverter.ToInt32(wav, 4));
        Assert.Equal(wav.Length - 44, BitConverter.ToInt32(wav, 40));
    }

    [Fact]
    public void LockChime_IsPcm16Mono22050()
    {
        var wav = WavSynthesizer.LockChime();
        Assert.Equal(1, BitConverter.ToInt16(wav, 20));
        Assert.Equal(1, BitConverter.ToInt16(wav, 22));
        Assert.Equal(WavSynthesizer.SampleRate, BitConverter.ToInt32(wav, 24));
        Assert.Equal(16, BitConverter.ToInt16(wav, 34));
    }

    [Fact]
    public void LockChime_DurationNear190Ms()
    {
        var wav = WavSynthesizer.LockChime();
        var sampleCount = (wav.Length - 44) / 2;
        Assert.InRange(sampleCount, (int)(0.17 * WavSynthesizer.SampleRate), (int)(0.21 * WavSynthesizer.SampleRate));
    }

    [Fact]
    public void LockChime_SamplesStayUnderAmplitudeCap()
    {
        var wav = WavSynthesizer.LockChime();
        for (var i = 44; i < wav.Length; i += 2)
        {
            var sample = BitConverter.ToInt16(wav, i);
            Assert.InRange(sample, (short)-19662, (short)19662);
        }
    }

    [Fact]
    public void LockChime_IsDeterministic()
    {
        Assert.Equal(WavSynthesizer.LockChime(), WavSynthesizer.LockChime());
    }

    [Fact]
    public void Meow_HasValidHeaderAndDurationNear600Ms()
    {
        var wav = WavSynthesizer.Meow();
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        var sampleCount = (wav.Length - 44) / 2;
        Assert.InRange(sampleCount, (int)(0.5 * WavSynthesizer.SampleRate), (int)(0.7 * WavSynthesizer.SampleRate));
    }

    [Fact]
    public void Meow_StaysUnderCapAndIsAudible()
    {
        var wav = WavSynthesizer.Meow();
        var peak = 0;
        for (var i = 44; i < wav.Length; i += 2)
        {
            var sample = Math.Abs((int)BitConverter.ToInt16(wav, i));
            Assert.True(sample <= 19662);
            peak = Math.Max(peak, sample);
        }
        Assert.True(peak > 8000);
    }

    [Fact]
    public void Meow_IsDeterministic()
    {
        Assert.Equal(WavSynthesizer.Meow(), WavSynthesizer.Meow());
    }

    [Fact]
    public void LockChime_ProducesAudibleSignal()
    {
        var wav = WavSynthesizer.LockChime();
        var peak = 0;
        for (var i = 44; i < wav.Length; i += 2)
        {
            peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(wav, i)));
        }
        Assert.True(peak > 8000);
    }
}
