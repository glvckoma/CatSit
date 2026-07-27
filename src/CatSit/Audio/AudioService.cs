using System.IO;
using System.Media;
using CatSit.Core.Audio;
using CatSit.Core.Settings;

namespace CatSit.Audio;

public sealed class AudioService : IDisposable
{
    private readonly AppSettings _settings;
    private readonly SoundPlayer _lockPlayer;

    public AudioService(AppSettings settings)
    {
        _settings = settings;
        _lockPlayer = CreatePlayer(WavSynthesizer.LockChime());
    }

    public void PlayLock()
    {
        if (_settings.SoundsEnabled) _lockPlayer.Play();
    }

    public void Dispose() => _lockPlayer.Dispose();

    private static SoundPlayer CreatePlayer(byte[] wav)
    {
        var player = new SoundPlayer(new MemoryStream(wav));
        player.Load();
        return player;
    }
}
