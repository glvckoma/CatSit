namespace CatSit.Core.Sprites;

public static class SpriteRasterizer
{
    public static uint[] Rasterize(SpriteFrame frame, bool mirror, CatTheme? theme = null)
    {
        var palette = theme?.ToPalette() ?? CatPalette.Colors;
        var width = frame.Width;
        var pixels = new uint[width * frame.Height];
        for (var y = 0; y < frame.Height; y++)
        {
            var row = frame.Rows[y];
            for (var x = 0; x < width; x++)
            {
                var sourceX = mirror ? width - 1 - x : x;
                pixels[y * width + x] = palette[row[sourceX]];
            }
        }
        return pixels;
    }
}
