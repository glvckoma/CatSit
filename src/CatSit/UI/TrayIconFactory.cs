using System.Drawing;
using System.Drawing.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class TrayIconFactory
{
    public static Icon Create()
    {
        var frame = AppIconArt.Icon;
        var pixels = SpriteRasterizer.Rasterize(frame, AppIconArt.Palette);
        using var bitmap = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);
        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(unchecked((int)pixels[y * frame.Width + x])));
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
