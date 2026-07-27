namespace CatSit.Core.Sprites;

public static class AppIconArt
{
    public static readonly IReadOnlyDictionary<char, uint> Palette = new Dictionary<char, uint>
    {
        ['.'] = 0x00000000,
        ['T'] = 0xFF232840,
        ['B'] = 0xFF6FD3B8,
        ['O'] = 0xFFF2BE88,
        ['D'] = 0xFFDFA05F,
        ['C'] = 0xFFFBEDDC,
        ['P'] = 0xFFF2A7A0,
        ['E'] = 0xFF4A342C,
        ['S'] = 0xFFF4F6F9
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
        "..BTTTOOOOOOOOOOOOOOOOOOOOTTTB..",
        "..BTTTOOOOEEOOOOOOOOEEOOOOTTTB..",
        "..BTTTOOOEOOEOOOOOOEOOEOOOTTTB..",
        "..BTSSOPPOOOOOOOOOOOOOOPPOSSTB..",
        "..BTTTOOOOOCCCCPPCCCCOOOOOTTTB..",
        "..BTTTOOOOOCCCCPPCCCCOOOOOTTTB..",
        "..BTSSOOOOOCCCECCECCCOOOOOSSTB..",
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
