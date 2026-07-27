using CatSit.Core.Sprites;

namespace CatSit.Core.Tests;

public class ColorMathTests
{
    [Fact]
    public void Darken_ReducesLuminanceAndKeepsAlpha()
    {
        const uint color = 0xFFE8963C;
        var darker = ColorMath.Darken(color, 0.25);
        Assert.True(ColorMath.Luminance(darker) < ColorMath.Luminance(color));
        Assert.Equal(0xFFu, darker >> 24);
    }

    [Fact]
    public void Lighten_IncreasesLuminanceAndKeepsAlpha()
    {
        const uint color = 0xFF3A3A42;
        var lighter = ColorMath.Lighten(color, 0.4);
        Assert.True(ColorMath.Luminance(lighter) > ColorMath.Luminance(color));
        Assert.Equal(0xFFu, lighter >> 24);
    }

    [Fact]
    public void DarkenAndLighten_ClampToChannelRange()
    {
        Assert.Equal(0xFF000000, ColorMath.Darken(0xFF000000, 1.5));
        Assert.Equal(0xFFFFFFFF, ColorMath.Lighten(0xFFFFFFFF, 1.5));
    }

    [Fact]
    public void Luminance_IsZeroForBlackAndOneForWhite()
    {
        Assert.Equal(0.0, ColorMath.Luminance(0xFF000000), 3);
        Assert.Equal(1.0, ColorMath.Luminance(0xFFFFFFFF), 3);
    }
}

public class CatThemeTests
{
    [Fact]
    public void FromBreed_OrangeTabby_KeepsOriginalFur()
    {
        Assert.Equal(0xFFE8963Cu, CatTheme.FromBreed(CatBreed.OrangeTabby).Fur);
    }

    [Fact]
    public void EveryBreed_HasDistinctFurAndReadableContrast()
    {
        var breeds = Enum.GetValues<CatBreed>();
        var furs = breeds.Select(b => CatTheme.FromBreed(b).Fur).ToList();
        Assert.Equal(furs.Count, furs.Distinct().Count());
        foreach (var breed in breeds)
        {
            var theme = CatTheme.FromBreed(breed);
            Assert.True(Math.Abs(ColorMath.Luminance(theme.Belly) - ColorMath.Luminance(theme.Stripes)) > 0.1);
        }
    }

    [Fact]
    public void EveryBreed_HasDisplayName()
    {
        foreach (var breed in Enum.GetValues<CatBreed>())
        {
            Assert.False(string.IsNullOrWhiteSpace(CatTheme.DisplayName(breed)));
        }
        Assert.Equal("Orange Tabby", CatTheme.DisplayName(CatBreed.OrangeTabby));
    }

    [Fact]
    public void FromTint_DerivesDarkerStripesAndLighterBelly()
    {
        var theme = CatTheme.FromTint(0xFF4C8BF5);
        Assert.Equal(0xFF4C8BF5u, theme.Fur);
        Assert.True(ColorMath.Luminance(theme.Stripes) < ColorMath.Luminance(theme.Fur));
        Assert.True(ColorMath.Luminance(theme.Belly) > ColorMath.Luminance(theme.Fur));
    }

    [Fact]
    public void FromTint_OnVeryDarkFur_StillProducesVisibleBelly()
    {
        var theme = CatTheme.FromTint(0xFF050508);
        Assert.True(ColorMath.Luminance(theme.Belly) > ColorMath.Luminance(theme.Fur) + 0.1);
    }

    [Fact]
    public void FromTint_OnVeryLightFur_StillProducesVisibleStripes()
    {
        var theme = CatTheme.FromTint(0xFFFDFDFD);
        Assert.True(ColorMath.Luminance(theme.Fur) - ColorMath.Luminance(theme.Stripes) > 0.1);
    }

    [Fact]
    public void ToPalette_CoversEverySpriteCharacter()
    {
        var palette = CatTheme.FromBreed(CatBreed.Tuxedo).ToPalette();
        foreach (var key in CatPalette.Colors.Keys)
        {
            Assert.True(palette.ContainsKey(key), $"palette missing '{key}'");
        }
        Assert.Equal(0x00000000u, palette['.']);
    }

    [Fact]
    public void ToPalette_MapsThemeColorsToSpriteSlots()
    {
        var theme = CatTheme.FromBreed(CatBreed.GrayTabby);
        var palette = theme.ToPalette();
        Assert.Equal(theme.Fur, palette['O']);
        Assert.Equal(theme.Stripes, palette['D']);
        Assert.Equal(theme.Belly, palette['C']);
        Assert.Equal(theme.Ears, palette['P']);
        Assert.Equal(theme.Eyes, palette['E']);
    }
}

public class ThemedRasterizerTests
{
    private static readonly SpriteFrame Sample = new([".O", "DC"]);

    [Fact]
    public void Rasterize_UsesProvidedTheme()
    {
        var theme = CatTheme.FromBreed(CatBreed.Tuxedo);
        var pixels = SpriteRasterizer.Rasterize(Sample, false, theme);
        Assert.Equal(0u, pixels[0]);
        Assert.Equal(theme.Fur, pixels[1]);
        Assert.Equal(theme.Stripes, pixels[2]);
        Assert.Equal(theme.Belly, pixels[3]);
    }

    [Fact]
    public void Rasterize_WithoutTheme_UsesDefaultPalette()
    {
        var pixels = SpriteRasterizer.Rasterize(Sample, false);
        Assert.Equal(CatPalette.Colors['O'], pixels[1]);
    }

    [Fact]
    public void Rasterize_ThemeAndMirrorCombine()
    {
        var theme = CatTheme.FromBreed(CatBreed.Black);
        var pixels = SpriteRasterizer.Rasterize(Sample, true, theme);
        Assert.Equal(theme.Fur, pixels[0]);
        Assert.Equal(0u, pixels[1]);
    }
}
