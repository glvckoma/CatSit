using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatSit.Core.Animation;
using CatSit.Core.Sprites;

namespace CatSit.Overlay;

public sealed class SpriteBitmapCache
{
    private readonly Dictionary<(SpriteFrame Frame, bool Mirrored), BitmapSource> _cache = [];
    private CatTheme _theme = CatTheme.FromBreed(CatBreed.OrangeTabby);

    public SpriteBitmapCache()
    {
        Rebuild(_theme);
    }

    public BitmapSource ZzzBitmap { get; private set; } = null!;

    public CatTheme Theme => _theme;

    public void Rebuild(CatTheme theme)
    {
        _theme = theme;
        _cache.Clear();
        foreach (var frame in CatSprites.AllCatFrames)
        {
            _cache[(frame, false)] = Build(frame, false, theme);
            _cache[(frame, true)] = Build(frame, true, theme);
        }
        ZzzBitmap = Build(CatSprites.Zzz, false, theme);
    }

    public BitmapSource Get(CatState state, int frameIndex, bool mirrored) =>
        _cache[(FrameFor(state, frameIndex), mirrored)];

    public BitmapSource Preview(CatTheme theme) => Build(CatSprites.Sitting, false, theme);

    private static SpriteFrame FrameFor(CatState state, int frameIndex) => state switch
    {
        CatState.WalkIn or CatState.WalkOut => CatSprites.Walk[frameIndex % CatSprites.Walk.Length],
        CatState.SitDown => CatSprites.SittingDown,
        CatState.Sitting => CatSprites.Sitting,
        CatState.LayDown or CatState.Laying => CatSprites.Laying,
        CatState.Sleeping => CatSprites.Sleep[frameIndex % CatSprites.Sleep.Length],
        _ => CatSprites.Walk[0]
    };

    private static BitmapSource Build(SpriteFrame frame, bool mirror, CatTheme theme)
    {
        var pixels = SpriteRasterizer.Rasterize(frame, mirror, theme);
        var bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Width * 4, 0);
        bitmap.Freeze();
        return bitmap;
    }
}
