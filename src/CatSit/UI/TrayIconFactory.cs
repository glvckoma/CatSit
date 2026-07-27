using System.Drawing;
using System.Drawing.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class TrayIconFactory
{
    public static Icon Create(CatTheme? theme = null)
    {
        var frame = CatSprites.Sitting;
        var pixels = SpriteRasterizer.Rasterize(frame, false, theme);
        using var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        var offsetY = (32 - frame.Height) / 2;
        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                bitmap.SetPixel(x, y + offsetY, Color.FromArgb(unchecked((int)pixels[y * frame.Width + x])));
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
