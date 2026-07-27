using CatSit.Core.Sprites;

namespace CatSit.Core.Tests;

public class SpriteRasterizerTests
{
    private static readonly SpriteFrame Sample = new([
        ".O",
        "DC"
    ]);

    [Fact]
    public void Rasterize_ReturnsWidthTimesHeightPixels()
    {
        Assert.Equal(4, SpriteRasterizer.Rasterize(Sample, false).Length);
    }

    [Fact]
    public void Rasterize_MapsTransparentAndColors()
    {
        var pixels = SpriteRasterizer.Rasterize(Sample, false);
        Assert.Equal(0u, pixels[0]);
        Assert.Equal(0xFFE8963C, pixels[1]);
        Assert.Equal(0xFFC2701F, pixels[2]);
        Assert.Equal(0xFFF6E3C1, pixels[3]);
    }

    [Fact]
    public void Rasterize_MirrorFlipsHorizontally()
    {
        var pixels = SpriteRasterizer.Rasterize(Sample, true);
        Assert.Equal(0xFFE8963C, pixels[0]);
        Assert.Equal(0u, pixels[1]);
        Assert.Equal(0xFFF6E3C1, pixels[2]);
        Assert.Equal(0xFFC2701F, pixels[3]);
    }

    [Fact]
    public void Rasterize_UnknownChar_Throws()
    {
        var bad = new SpriteFrame(["?"]);
        Assert.Throws<KeyNotFoundException>(() => SpriteRasterizer.Rasterize(bad, false));
    }
}

public class CatSpritesTests
{
    [Fact]
    public void AllCatFrames_Are32By24WithPaletteCharsOnly()
    {
        var frames = CatSprites.AllCatFrames.ToList();
        Assert.NotEmpty(frames);
        foreach (var frame in frames)
        {
            Assert.Equal(24, frame.Height);
            Assert.All(frame.Rows, row => Assert.Equal(32, row.Length));
            Assert.All(frame.Rows, row => Assert.All(row, c => Assert.Contains(c, CatPalette.Colors.Keys)));
        }
    }

    [Fact]
    public void Walk_HasFourFrames_WithDistinctStrides()
    {
        Assert.Equal(4, CatSprites.Walk.Length);
        Assert.NotEqual(CatSprites.Walk[0].Rows, CatSprites.Walk[2].Rows);
    }

    [Fact]
    public void Sleep_HasTwoDistinctFrames()
    {
        Assert.Equal(2, CatSprites.Sleep.Length);
        Assert.NotEqual(CatSprites.Sleep[0].Rows, CatSprites.Sleep[1].Rows);
    }

    [Fact]
    public void Zzz_IsEightByEightPaletteOnly()
    {
        Assert.Equal(8, CatSprites.Zzz.Height);
        Assert.All(CatSprites.Zzz.Rows, row => Assert.Equal(8, row.Length));
        Assert.Contains(CatSprites.Zzz.Rows, row => row.Contains('Z'));
    }

    [Fact]
    public void CatFrames_HaveVisibleCatAndTransparentBackground()
    {
        foreach (var frame in CatSprites.AllCatFrames)
        {
            var opaque = frame.Rows.Sum(row => row.Count(c => c != '.'));
            var total = frame.Width * frame.Height;
            Assert.InRange(opaque, total / 8, total * 3 / 4);
        }
    }
}
