namespace CatSit.Core.Sprites;

public static class SpriteRasterizer
{
    public static uint[] Rasterize(SpriteFrame frame, bool mirror, CatTheme? theme = null) =>
        Rasterize(frame, theme?.ToPalette() ?? CatPalette.Colors, mirror);

    public static uint[] Rasterize(SpriteFrame frame, IReadOnlyDictionary<char, uint> palette, bool mirror = false)
    {
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

    public static uint[] UpscaleNearest(uint[] pixels, int width, int height, int factor)
    {
        if (factor < 1) throw new ArgumentOutOfRangeException(nameof(factor));
        if (factor == 1) return pixels;
        var scaledWidth = width * factor;
        var result = new uint[scaledWidth * height * factor];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = pixels[y * width + x];
                for (var dy = 0; dy < factor; dy++)
                {
                    var rowStart = (y * factor + dy) * scaledWidth + x * factor;
                    for (var dx = 0; dx < factor; dx++)
                    {
                        result[rowStart + dx] = color;
                    }
                }
            }
        }
        return result;
    }
}
