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

public class UpscaleNearestTests
{
    private static readonly uint[] TwoByTwo = [0xFF000001, 0xFF000002, 0xFF000003, 0xFF000004];

    [Fact]
    public void FactorOne_ReturnsEquivalentPixels()
    {
        var result = SpriteRasterizer.UpscaleNearest(TwoByTwo, 2, 2, 1);
        Assert.Equal(TwoByTwo, result);
    }

    [Fact]
    public void FactorTwo_ProducesQuadrupleSizedBlocks()
    {
        var result = SpriteRasterizer.UpscaleNearest(TwoByTwo, 2, 2, 2);
        Assert.Equal(16, result.Length);
        var width = 2 * 2;
        Assert.Equal(0xFF000001u, result[0 * width + 0]);
        Assert.Equal(0xFF000001u, result[1 * width + 1]);
        Assert.Equal(0xFF000002u, result[0 * width + 2]);
        Assert.Equal(0xFF000002u, result[1 * width + 3]);
        Assert.Equal(0xFF000003u, result[2 * width + 0]);
        Assert.Equal(0xFF000004u, result[3 * width + 3]);
    }

    [Fact]
    public void FactorFour_PreservesEveryOriginalPixelAsASolidBlock()
    {
        var result = SpriteRasterizer.UpscaleNearest(TwoByTwo, 2, 2, 4);
        var width = 2 * 4;
        Assert.Equal(64, result.Length);
        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                Assert.Equal(0xFF000001u, result[y * width + x]);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveFactor_Throws(int factor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SpriteRasterizer.UpscaleNearest(TwoByTwo, 2, 2, factor));
    }
}

public class CatSpritesTests
{
    [Fact]
    public void AllCatFrames_MatchFrameSizeWithPaletteCharsOnly()
    {
        var frames = CatSprites.AllCatFrames.ToList();
        Assert.NotEmpty(frames);
        foreach (var frame in frames)
        {
            Assert.Equal(CatSprites.FrameHeight, frame.Height);
            Assert.All(frame.Rows, row => Assert.Equal(CatSprites.FrameWidth, row.Length));
            Assert.All(frame.Rows, row => Assert.All(row, c => Assert.Contains(c, CatPalette.Colors.Keys)));
        }
    }

    [Fact]
    public void EveryCatFrame_HasPointyEarsWithPinkInnerAndWhiskers()
    {
        foreach (var frame in CatSprites.AllCatFrames)
        {
            Assert.Contains(frame.Rows, row => row.Contains('P'));
            Assert.Contains(frame.Rows, row => row.Contains('S'));
        }
    }

    [Fact]
    public void EveryCatFrame_HasATail()
    {
        foreach (var frame in CatSprites.AllCatFrames)
        {
            Assert.Contains(frame.Rows, row => row.Contains('L'));
        }
    }

    [Fact]
    public void AwakeFrames_HaveTwoEyesWithHighlights()
    {
        foreach (var frame in new[] { CatSprites.Sitting, CatSprites.Laying, CatSprites.Walk[0] })
        {
            Assert.Contains(frame.Rows, row => row.Contains('I'));
            Assert.Contains(frame.Rows, row => row.Contains('W'));
        }
    }

    [Fact]
    public void SleepingFrames_HaveClosedEyes()
    {
        foreach (var frame in CatSprites.Sleep)
        {
            Assert.DoesNotContain(frame.Rows, row => row.Contains('I'));
            Assert.Contains(frame.Rows, row => row.Contains('E'));
        }
    }

    [Fact]
    public void AppIcon_Is32SquarePaletteOnlyWithTransparentCorners()
    {
        Assert.Equal(32, AppIconArt.Icon.Width);
        Assert.Equal(32, AppIconArt.Icon.Height);
        Assert.All(AppIconArt.Icon.Rows, row => Assert.All(row, c => Assert.Contains(c, AppIconArt.Palette.Keys)));
        Assert.Equal('.', AppIconArt.Icon.Rows[2][0]);
        Assert.Equal('.', AppIconArt.Icon.Rows[29][31]);
    }

    [Fact]
    public void AppIcon_HasTilePointyEarsWhiskersAndClosedEyes()
    {
        var rows = AppIconArt.Icon.Rows;
        Assert.Contains(rows, row => row.Contains('B'));
        Assert.Contains(rows, row => row.Contains('S'));
        Assert.Contains(rows, row => row.Contains("EE"));
        Assert.DoesNotContain(rows, row => row.Contains('W'));
        Assert.Contains('O', rows[0]);
        Assert.DoesNotContain('T', rows[0]);
    }

    [Fact]
    public void Rasterize_WithExplicitPalette_UsesIt()
    {
        var frame = new SpriteFrame(["XY"]);
        var palette = new Dictionary<char, uint> { ['X'] = 0xFF112233, ['Y'] = 0xFF445566 };
        var pixels = SpriteRasterizer.Rasterize(frame, palette);
        Assert.Equal(0xFF112233u, pixels[0]);
        Assert.Equal(0xFF445566u, pixels[1]);
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
