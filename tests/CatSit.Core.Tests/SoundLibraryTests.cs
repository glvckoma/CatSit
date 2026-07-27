using CatSit.Core.Audio;

namespace CatSit.Core.Tests;

public class SoundLibraryTests
{
    private static int SampleCount(byte[] wav) => (wav.Length - 44) / 2;

    private static int Peak(byte[] wav)
    {
        var peak = 0;
        for (var i = 44; i < wav.Length; i += 2)
        {
            peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(wav, i)));
        }
        return peak;
    }

    public static IEnumerable<object[]> AllVariants()
    {
        yield return [WavSynthesizer.Beep()];
        yield return [WavSynthesizer.Thud()];
        yield return [WavSynthesizer.Alarm()];
        yield return [WavSynthesizer.Chirp()];
        yield return [WavSynthesizer.Trill()];
        yield return [WavSynthesizer.Hiss()];
        yield return [WavSynthesizer.Shake()];
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void EveryVariant_IsValidAudibleWav(byte[] wav)
    {
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        Assert.Equal(wav.Length - 44, BitConverter.ToInt32(wav, 40));
        Assert.Equal(WavSynthesizer.SampleRate, BitConverter.ToInt32(wav, 24));
        Assert.InRange(SampleCount(wav), WavSynthesizer.SampleRate / 20, WavSynthesizer.SampleRate * 2);
        Assert.InRange(Peak(wav), 6000, 19662);
    }

    [Fact]
    public void NoiseBasedSounds_AreDeterministic()
    {
        Assert.Equal(WavSynthesizer.Hiss(), WavSynthesizer.Hiss());
        Assert.Equal(WavSynthesizer.Shake(), WavSynthesizer.Shake());
    }

    [Fact]
    public void Variants_SoundDifferentFromEachOther()
    {
        Assert.NotEqual(WavSynthesizer.Beep(), WavSynthesizer.Thud());
        Assert.NotEqual(WavSynthesizer.Meow(), WavSynthesizer.Chirp());
        Assert.NotEqual(WavSynthesizer.Hiss(), WavSynthesizer.Shake());
    }

    [Fact]
    public void For_MapsEachEnumToAudioOrEmptyForNone()
    {
        Assert.Empty(WavSynthesizer.For(LockSound.None));
        Assert.Empty(WavSynthesizer.For(CatSound.None));
        Assert.Empty(WavSynthesizer.For(DeterrentSound.None));
        Assert.Empty(WavSynthesizer.For(LockSound.Custom));
        Assert.Equal(WavSynthesizer.LockChime(), WavSynthesizer.For(LockSound.Chime));
        Assert.Equal(WavSynthesizer.Meow(), WavSynthesizer.For(CatSound.Meow));
        Assert.Equal(WavSynthesizer.Hiss(), WavSynthesizer.For(DeterrentSound.Hiss));
    }

    [Fact]
    public void For_CoversEveryBuiltInVariant()
    {
        foreach (var sound in Enum.GetValues<LockSound>().Where(s => s is not (LockSound.None or LockSound.Custom)))
        {
            Assert.NotEmpty(WavSynthesizer.For(sound));
        }
        foreach (var sound in Enum.GetValues<CatSound>().Where(s => s is not (CatSound.None or CatSound.Custom)))
        {
            Assert.NotEmpty(WavSynthesizer.For(sound));
        }
        foreach (var sound in Enum.GetValues<DeterrentSound>().Where(s => s is not (DeterrentSound.None or DeterrentSound.Custom)))
        {
            Assert.NotEmpty(WavSynthesizer.For(sound));
        }
    }

    [Fact]
    public void SoundNames_AreHumanReadable()
    {
        Assert.Equal("Silent", SoundNames.For(LockSound.None));
        Assert.Equal("Shake can", SoundNames.For(DeterrentSound.Shake));
        foreach (var sound in Enum.GetValues<CatSound>())
        {
            Assert.False(string.IsNullOrWhiteSpace(SoundNames.For(sound)));
        }
    }
}

public class WavFileTests
{
    [Fact]
    public void IsPcm16_TrueForSynthesizedAudio()
    {
        Assert.True(WavFile.IsPcm16(WavSynthesizer.LockChime()));
    }

    [Fact]
    public void IsPcm16_FalseForGarbageOrTruncatedData()
    {
        Assert.False(WavFile.IsPcm16([1, 2, 3]));
        Assert.False(WavFile.IsPcm16(System.Text.Encoding.ASCII.GetBytes("NOTAWAVEFILEATALLBUTLONGENOUGHTOPASSLENGTHCHECKS")));
        Assert.False(WavFile.IsPcm16([]));
    }

    [Fact]
    public void ApplyVolume_HalvesSampleAmplitude()
    {
        var original = WavSynthesizer.LockChime();
        var quieter = WavFile.ApplyVolume(original, 0.5);
        Assert.Equal(original.Length, quieter.Length);
        for (var i = 44; i < original.Length; i += 2)
        {
            var before = BitConverter.ToInt16(original, i);
            var after = BitConverter.ToInt16(quieter, i);
            Assert.InRange(Math.Abs(after), 0, Math.Abs((int)before));
            Assert.InRange(Math.Abs(before / 2 - after), 0, 1);
        }
    }

    [Fact]
    public void ApplyVolume_FullVolumeIsUnchanged()
    {
        var original = WavSynthesizer.Meow();
        Assert.Equal(original, WavFile.ApplyVolume(original, 1.0));
    }

    [Fact]
    public void ApplyVolume_ZeroProducesSilence()
    {
        var silent = WavFile.ApplyVolume(WavSynthesizer.Meow(), 0);
        for (var i = 44; i < silent.Length; i += 2)
        {
            Assert.Equal(0, BitConverter.ToInt16(silent, i));
        }
    }

    [Fact]
    public void ApplyVolume_ClampsOutOfRangeVolume()
    {
        var original = WavSynthesizer.LockChime();
        Assert.Equal(original, WavFile.ApplyVolume(original, 5.0));
        Assert.Equal(WavFile.ApplyVolume(original, 0), WavFile.ApplyVolume(original, -3.0));
    }

    [Fact]
    public void ApplyVolume_LeavesUnsupportedDataUntouched()
    {
        byte[] garbage = [9, 9, 9, 9];
        Assert.Equal(garbage, WavFile.ApplyVolume(garbage, 0.5));
    }
}
