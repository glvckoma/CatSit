namespace CatSit.Core.Sprites;

public static class AppIconArt
{
    public static readonly IReadOnlyDictionary<char, uint> Palette = new Dictionary<char, uint>
    {
        ['.'] = 0x00000000,
        ['T'] = 0xFF232840,
        ['B'] = 0xFF6FD3B8,
        ['O'] = 0xFFE8963C,
        ['D'] = 0xFFC2701F,
        ['C'] = 0xFFF6E3C1,
        ['P'] = 0xFFE8A0A8,
        ['G'] = 0xFF86C06C,
        ['E'] = 0xFF1B140F,
        ['W'] = 0xFFFFFFFF,
        ['S'] = 0xFFF0F3F8
    };

    public static readonly SpriteFrame Icon = new([
        ".......O................O.......",
        "......OOO..............OOO......",
        "....BBOOOOBBBBBBBBBBBBOOOOBB....",
        "...BTTOPPPOTTTTTTTTTTOPPPOTTB...",
        "..BTTTOOPPOOTTTTTTTTOOPPOOTTTB..",
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTOOOODDOOODDOOODDOOOOTTTB..",
        "..BTTTOOODOOODDOODDOOODOOOTTTB..",
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTOOOGGGGOOOOOOGGGGOOOTTTB..",
        "..BTTTOOOGEWGOOOOOOGEWGOOOTTTB..",
        "..BTTTOOOGGGGOOOOOOGGGGOOOTTTB..",
        "..BTSSOOOOOOOOOOOOOOOOOOOOSSTB..",
        "..BTTTOOOOCCCCPPPPCCCCOOOOTTTB..",
        "..BTTTOOOOCCCCCEECCCCCOOOOTTTB..",
        "..BTSSOOOOCCCECCCCECCCOOOOSSTB..",
        "..BTTTOOOOCCCCCCCCCCCCOOOOTTTB..",
        "..BTTTOOOOOOCCCCCCCCOOOOOOTTTB..",
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTODOOOOOOOOOOOOOOOODOTTTB..",
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTTOOOOOOOOOOOOOOOOOOTTTTB..",
        "..BTTTTTOOOOOOOOOOOOOOOOTTTTTB..",
        "..BTTTTTTTOOOOOOOOOOOOTTTTTTTB..",
        "..BTTTTTTTTTTTTTTTTTTTTTTTTTTB..",
        "..BTTTTTTTTTTTTTTTTTTTTTTTTTTB..",
        "..BTTTTTTTTTTTTTTTTTTTTTTTTTTB..",
        "...BTTTTTTTTTTTTTTTTTTTTTTTTB...",
        "....BBBBBBBBBBBBBBBBBBBBBBBB....",
        "................................",
        "................................"
    ]);
}
