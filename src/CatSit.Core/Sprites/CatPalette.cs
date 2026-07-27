namespace CatSit.Core.Sprites;

public static class CatPalette
{
    public static readonly IReadOnlyDictionary<char, uint> Colors = new Dictionary<char, uint>
    {
        ['.'] = 0x00000000,
        ['O'] = 0xFFE8963C,
        ['L'] = 0xFFF3B269,
        ['D'] = 0xFFC2701F,
        ['C'] = 0xFFF6E3C1,
        ['P'] = 0xFFE8A0A8,
        ['I'] = 0xFF6FA84B,
        ['E'] = 0xFF1B140F,
        ['W'] = 0xFFFFFFFF,
        ['S'] = 0xFFFFFFFF,
        ['Z'] = 0xFF9FC4E8
    };
}
