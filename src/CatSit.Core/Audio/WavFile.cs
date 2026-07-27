using System.Text;

namespace CatSit.Core.Audio;

public static class WavFile
{
    private const int HeaderSize = 44;

    public static bool IsPcm16(byte[] wav)
    {
        if (wav.Length <= HeaderSize) return false;
        if (Encoding.ASCII.GetString(wav, 0, 4) != "RIFF") return false;
        if (Encoding.ASCII.GetString(wav, 8, 4) != "WAVE") return false;
        if (Encoding.ASCII.GetString(wav, 12, 4) != "fmt ") return false;
        if (BitConverter.ToInt16(wav, 20) != 1) return false;
        if (BitConverter.ToInt16(wav, 34) != 16) return false;
        return Encoding.ASCII.GetString(wav, 36, 4) == "data";
    }

    public static byte[] ApplyVolume(byte[] wav, double volume)
    {
        var scale = Math.Clamp(volume, 0, 1);
        if (scale >= 1.0 || !IsPcm16(wav)) return wav;
        var scaled = (byte[])wav.Clone();
        for (var i = HeaderSize; i + 1 < scaled.Length; i += 2)
        {
            var sample = (short)Math.Round(BitConverter.ToInt16(scaled, i) * scale);
            BitConverter.TryWriteBytes(scaled.AsSpan(i, 2), sample);
        }
        return scaled;
    }
}
