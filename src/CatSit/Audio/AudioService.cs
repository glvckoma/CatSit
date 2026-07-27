using System.IO;
using System.Media;
using CatSit.Core.Audio;
using CatSit.Core.Settings;

namespace CatSit.Audio;

public sealed class AudioService : IDisposable
{
    private readonly AppSettings _settings;
    private SoundPlayer? _lockPlayer;
    private SoundPlayer? _catPlayer;
    private SoundPlayer? _deterrentPlayer;

    public AudioService(AppSettings settings)
    {
        _settings = settings;
        Rebuild();
    }

    public void Rebuild()
    {
        var volume = _settings.Volume / 100.0;
        Swap(ref _lockPlayer, Resolve(WavSynthesizer.For(_settings.LockSound), _settings.LockSound == LockSound.Custom, _settings.CustomLockSoundPath, volume));
        Swap(ref _catPlayer, Resolve(WavSynthesizer.For(_settings.CatSound), _settings.CatSound == CatSound.Custom, _settings.CustomCatSoundPath, volume));
        Swap(ref _deterrentPlayer, Resolve(WavSynthesizer.For(_settings.DeterrentSound), _settings.DeterrentSound == DeterrentSound.Custom, _settings.CustomDeterrentSoundPath, volume));
    }

    public void PlayLock() => Play(_lockPlayer);

    public void PlayCat() => Play(_catPlayer);

    public void PlayDeterrent() => Play(_deterrentPlayer);

    public void Preview(byte[] wav)
    {
        if (wav.Length == 0) return;
        using var player = new SoundPlayer(new MemoryStream(WavFile.ApplyVolume(wav, _settings.Volume / 100.0)));
        player.Play();
    }

    public void PreviewFile(string path)
    {
        var wav = ReadFile(path);
        if (wav is not null) Preview(wav);
    }

    public void Dispose()
    {
        _lockPlayer?.Dispose();
        _catPlayer?.Dispose();
        _deterrentPlayer?.Dispose();
    }

    private void Play(SoundPlayer? player)
    {
        if (!_settings.SoundsEnabled || player is null) return;
        try
        {
            player.Play();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void Swap(ref SoundPlayer? slot, SoundPlayer? replacement)
    {
        slot?.Dispose();
        slot = replacement;
    }

    private static SoundPlayer? Resolve(byte[] builtIn, bool useCustom, string? customPath, double volume)
    {
        var wav = useCustom ? ReadFile(customPath) : builtIn;
        if (wav is null || wav.Length == 0) return null;
        try
        {
            var player = new SoundPlayer(new MemoryStream(WavFile.ApplyVolume(wav, volume)));
            player.Load();
            return player;
        }
        catch (Exception e) when (e is InvalidOperationException or FileFormatException)
        {
            return null;
        }
    }

    private static byte[]? ReadFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
