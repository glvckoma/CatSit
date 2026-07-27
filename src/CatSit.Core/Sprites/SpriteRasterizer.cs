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
}
