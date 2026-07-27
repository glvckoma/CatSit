namespace CatSit.Core.Sprites;

public static class CatPalette
{
    public static readonly IReadOnlyDictionary<char, uint> Colors = new Dictionary<char, uint>
    {
        ['.'] = 0x00000000,
        ['O'] = 0xFFE8963C,
        ['D'] = 0xFFC2701F,
        ['C'] = 0xFFF6E3C1,
        ['P'] = 0xFFE8A0A8,
        ['W'] = 0xFFFFFFFF,
        ['E'] = 0xFF241A12,
        ['Z'] = 0xFF9FC4E8
    };
}
