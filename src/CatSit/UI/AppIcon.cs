using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class AppIcon
{
    private const int UpscaleFactor = 4;

    private static readonly Lazy<BitmapSource> Bitmap = new(Build);

    public static BitmapSource Image => Bitmap.Value;

    private static BitmapSource Build()
    {
        var frame = AppIconArt.Icon;
        var pixels = SpriteRasterizer.Rasterize(frame, AppIconArt.Palette);
        var scaled = SpriteRasterizer.UpscaleNearest(pixels, frame.Width, frame.Height, UpscaleFactor);
        var width = frame.Width * UpscaleFactor;
        var height = frame.Height * UpscaleFactor;
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, width, height), scaled, width * 4, 0);
        bitmap.Freeze();
        return bitmap;
    }
}
