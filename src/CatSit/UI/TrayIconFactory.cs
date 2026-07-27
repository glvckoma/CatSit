using System.Drawing;
using System.Drawing.Imaging;
using CatSit.Core.Sprites;

namespace CatSit.UI;

public static class TrayIconFactory
{
    public static Icon Create(CatTheme? theme = null)
    {
        var frame = CatSprites.IconFace;
        var pixels = SpriteRasterizer.Rasterize(frame, false, theme);
        using var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                var color = Color.FromArgb(unchecked((int)pixels[y * frame.Width + x]));
                bitmap.SetPixel(x * 2, y * 2, color);
                bitmap.SetPixel(x * 2 + 1, y * 2, color);
                bitmap.SetPixel(x * 2, y * 2 + 1, color);
                bitmap.SetPixel(x * 2 + 1, y * 2 + 1, color);
            }
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
