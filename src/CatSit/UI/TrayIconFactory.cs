using System.Drawing;
using System.Drawing.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class TrayIconFactory
{
    private const int UpscaleFactor = 4;

    public static Icon Create()
    {
        var frame = AppIconArt.Icon;
        var pixels = SpriteRasterizer.Rasterize(frame, AppIconArt.Palette);
        var scaled = SpriteRasterizer.UpscaleNearest(pixels, frame.Width, frame.Height, UpscaleFactor);
        var width = frame.Width * UpscaleFactor;
        var height = frame.Height * UpscaleFactor;
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(unchecked((int)scaled[y * width + x])));
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
