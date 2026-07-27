using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class AppIcon
{
    private static readonly Lazy<BitmapSource> Bitmap = new(Build);

    public static BitmapSource Image => Bitmap.Value;

    public static CatTheme Theme => CatTheme.FromBreed(CatBreed.OrangeTabby);

    private static BitmapSource Build()
    {
        var frame = CatSprites.IconFace;
        var pixels = SpriteRasterizer.Rasterize(frame, false, Theme);
        var bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Width * 4, 0);
        bitmap.Freeze();
        return bitmap;
    }
}
