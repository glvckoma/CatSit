namespace CatSit.Core.Sprites;

public enum CatBreed
{
    OrangeTabby,
    GrayTabby,
    Tuxedo,
    Siamese,
    Black,
    White,
    Calico
}

public sealed record CatTheme(uint Fur, uint Stripes, uint Belly, uint Ears, uint Eyes)
{
    private const uint DefaultEars = 0xFFE8A0A8;
    private const uint DefaultEyes = 0xFF241A12;

    public static CatTheme FromBreed(CatBreed breed) => breed switch
    {
        CatBreed.OrangeTabby => new CatTheme(0xFFE8963C, 0xFFC2701F, 0xFFF6E3C1, DefaultEars, DefaultEyes),
        CatBreed.GrayTabby => new CatTheme(0xFF9AA3AD, 0xFF6E7783, 0xFFE8EDF2, 0xFFDFA6AC, DefaultEyes),
        CatBreed.Tuxedo => new CatTheme(0xFF2F2F36, 0xFF1B1B21, 0xFFF4F4F0, 0xFFD9959B, DefaultEyes),
        CatBreed.Siamese => new CatTheme(0xFFE0CDB2, 0xFF6B4A38, 0xFFF9F1E2, 0xFFD9A0A6, 0xFF2B4C6B),
        CatBreed.Black => new CatTheme(0xFF3A3A42, 0xFF23232A, 0xFF5A5A66, 0xFFB98A90, 0xFF8FA86B),
        CatBreed.White => new CatTheme(0xFFF2F0EA, 0xFFCFC9BC, 0xFFFFFFFF, 0xFFEDAFB5, 0xFF3F6C8F),
        CatBreed.Calico => new CatTheme(0xFFF0C89A, 0xFF3A3A42, 0xFFFBF3E7, DefaultEars, 0xFF6B4A2F),
        _ => FromBreed(CatBreed.OrangeTabby)
    };

    public static CatTheme FromTint(uint fur)
    {
        var luminance = ColorMath.Luminance(fur);
        var stripes = luminance < 0.2 ? ColorMath.Darken(fur, 0.45) : ColorMath.Darken(fur, 0.3);
        if (ColorMath.Luminance(fur) - ColorMath.Luminance(stripes) < 0.1) stripes = ColorMath.Darken(fur, 0.6);
        var belly = ColorMath.Lighten(fur, luminance > 0.8 ? 0.55 : 0.45);
        if (ColorMath.Luminance(belly) - luminance < 0.12) belly = ColorMath.Lighten(fur, 0.75);
        return new CatTheme(fur, stripes, belly, DefaultEars, DefaultEyes);
    }

    public static string DisplayName(CatBreed breed) => breed switch
    {
        CatBreed.OrangeTabby => "Orange Tabby",
        CatBreed.GrayTabby => "Gray Tabby",
        CatBreed.Tuxedo => "Tuxedo",
        CatBreed.Siamese => "Siamese",
        CatBreed.Black => "Black",
        CatBreed.White => "White",
        CatBreed.Calico => "Calico",
        _ => breed.ToString()
    };

    public IReadOnlyDictionary<char, uint> ToPalette() => new Dictionary<char, uint>
    {
        ['.'] = 0x00000000,
        ['O'] = Fur,
        ['D'] = Stripes,
        ['C'] = Belly,
        ['P'] = Ears,
        ['W'] = 0xFFFFFFFF,
        ['E'] = Eyes,
        ['Z'] = 0xFF9FC4E8
    };
}
