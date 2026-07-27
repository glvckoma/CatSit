using System.IO;
using System.Media;
using CatSit.Core.Audio;
using CatSit.Core.Settings;

namespace CatSit.Audio;

public sealed class AudioService : IDisposable
{
    private readonly AppSettings _settings;
    private readonly SoundPlayer _lockPlayer;
    private readonly SoundPlayer _meowPlayer;

    public AudioService(AppSettings settings)
    {
        _settings = settings;
        _lockPlayer = CreatePlayer(WavSynthesizer.LockChime());
        _meowPlayer = CreatePlayer(WavSynthesizer.Meow());
    }

    public void PlayLock()
    {
        if (_settings.SoundsEnabled) _lockPlayer.Play();
    }

    public void PlayMeow()
    {
        if (_settings.SoundsEnabled) _meowPlayer.Play();
    }

    public void Dispose()
    {
        _lockPlayer.Dispose();
        _meowPlayer.Dispose();
    }

    private static SoundPlayer CreatePlayer(byte[] wav)
    {
        var player = new SoundPlayer(new MemoryStream(wav));
        player.Load();
        return player;
    }
}
