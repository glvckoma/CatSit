namespace CatSit.Core.Sprites;

public static class CatSprites
{
    private const string Blank = "................................";

    private static readonly string[] WalkBody =
    [
        "....................DD.....DD...",
        "...................OPPO...OPPO..",
        "...................OOOOOOOOOOO..",
        "..................OOOOOOOOOOOOO.",
        "..................OODOODOOOOOOO.",
        ".DD..............OOOOOOOOOOOOOO.",
        ".OO..............OOOOEWOOOEWOOO.",
        ".OO..............OOOOEEOOOEEOOO.",
        "..OO.............OOOOOCCCCCCCOO.",
        "..OO.............OOOOCCCPPCCCCO.",
        "..OO.OOOOOOOOOOOOOOOOCCCECCCCCO.",
        "..OOOOOOOOOOOOOOOOOOOCCCCCCCOO..",
        "...OODOOODOOODOOOOCCC...........",
        "..OOODOOODOOODOOOCCCC...........",
        "..OOOOOOOOOOOOOOCCCCC...........",
        "..OOOOOOOOOOOOOCCCCC............",
        "...OOOOOOOOOOOCCCCC.............",
        "....OOOOOOOOOOOOOOO............."
    ];

    private const string StrideALegs = "....DD.OO.....DD.OO.............";
    private const string StrideAPaws = "....DD.OOO....DD.OOO............";
    private const string NeutralLegs = ".....OO.........OO..............";
    private const string NeutralPaws = ".....OOO........OOO.............";
    private const string StrideBLegs = "....OO.DD.....OO.DD.............";
    private const string StrideBPaws = "....OOO.DD....OOO.DD............";

    public static readonly SpriteFrame[] Walk =
    [
        WalkFrame(StrideALegs, StrideAPaws),
        WalkFrame(NeutralLegs, NeutralPaws),
        WalkFrame(StrideBLegs, StrideBPaws),
        WalkFrame(NeutralLegs, NeutralPaws)
    ];

    public static readonly SpriteFrame Crouch = new([
        Blank,
        Blank,
        .. WalkBody,
        NeutralLegs,
        NeutralLegs,
        NeutralLegs,
        NeutralPaws
    ]);

    public static readonly SpriteFrame Sitting = new([
        "....................DD.....DD...",
        "...................OPPO...OPPO..",
        "...................OOOOOOOOOOO..",
        "..................OOOOOOOOOOOOO.",
        "..................OODOODOOOOOOO.",
        ".................OOOOEWOOOEWOOO.",
        ".................OOOOEEOOOEEOOO.",
        ".................OOOOOCCCCCCCOO.",
        ".................OOOOCCCPPCCCCO.",
        ".................OOOOCCCECCCCCO.",
        "..................OOOCCCCCCCOO..",
        "........OOOOOOOOOOOOCCCCO.......",
        "......OOOODOOOODOOOCCCCCO.......",
        ".....OOOOODOOOODOOOCCCCCO.......",
        "....OOOOOOODOOOODOOCCCCCO.......",
        "....OOOOOOOOOOOOOOOCCCCCO.......",
        "....OOOOOOOOOOOOOOOCCCCO........",
        "....OOOOOOOOOOOOOOOCCCO.........",
        ".....OOOOOOOOOOOOODDOO..........",
        ".....OOOOOOOOOOOO.DDOO..........",
        "......OOOOOOOOOO..DDOO..........",
        "...DOOOOOOOOOOO...DDOO..........",
        "..DOOOOOOO........DDOO..........",
        "...DDD............DDOOO........."
    ]);

    public static readonly SpriteFrame Laying = new([
        Blank,
        Blank,
        Blank,
        Blank,
        Blank,
        "....................DD.....DD...",
        "...................OPPO...OPPO..",
        "...................OOOOOOOOOOO..",
        "..................OOOOOOOOOOOOO.",
        "..................OODOODOOOOOOO.",
        ".................OOOOEWOOOEWOOO.",
        ".................OOOOEEOOOEEOOO.",
        ".................OOOOOCCCCCCCOO.",
        "....OOOOOOOOOOOOOOOOOCCCPPCCCCO.",
        "...OOOOOOOOOOOOOOOOOOCCCECCCCCO.",
        "..OOOOOOOOOOOOOOOOOOOCCCCCCCOO..",
        "..OOODOOODOOODOOODOOOCCCC.......",
        ".OOOODOOODOOODOOODOOOCCCCC......",
        ".OOOOOOOOOOOOOOOOOOOOCCCCC......",
        ".OOOOOOOOOOOOOOOOOOOOCCCC.......",
        "..OOOOOOOOOOOOOOOOOOOCCC........",
        "..DOOOOOOOOOOOOOOOOOOCC.........",
        "...DDOOOOOOOOOOOOOOOO...........",
        "......DDDDDDDDDDDDDDDD.........."
    ]);

    public static readonly SpriteFrame[] Sleep =
    [
        SleepFrame("....OOOOOOOOOOOOOOOOOOOOOOOOOOO."),
        SleepFrame("......OOOOOOOOOOOOOOOOOOOOOOOOO.")
    ];

    public static readonly SpriteFrame Zzz = new([
        "........",
        ".ZZZZZ..",
        "....Z...",
        "...Z....",
        "..Z.....",
        ".ZZZZZ..",
        "........",
        "........"
    ]);

    public static IEnumerable<SpriteFrame> AllCatFrames =>
        [.. Walk, Crouch, Sitting, Laying, .. Sleep];

    private static SpriteFrame WalkFrame(string legs, string paws) =>
        new([.. WalkBody, legs, legs, legs, legs, legs, paws]);

    private static SpriteFrame SleepFrame(string backRow) =>
        new([
            Blank,
            Blank,
            Blank,
            Blank,
            Blank,
            Blank,
            Blank,
            Blank,
            Blank,
            "....................DD.....DD...",
            "...................OPPO...OPPO..",
            "...................OOOOOOOOOOO..",
            backRow,
            "...OOOOOOOOOOOOOOOOOOEEOOOEEOOO.",
            "..OOOOOOOOOOOOOOOOOOOOCCCCCCCOO.",
            "..OOOOOOOOOOOOOOOOOOOCCCPPCCCCO.",
            ".OOOODOOODOOODOOODOOOCCCCCCCOO..",
            ".OOOOOOOOOOOOOOOOOOOOCCCCCC.....",
            ".OOOOOOOOOOOOOOOOOOOOCCCCC......",
            ".OOOOOOOOOOOOOOOOOOOOCCCC.......",
            "..OOOOOOOOOOOOOOOOOOOCCC........",
            "..DOOOOOOOOOOOOOOOOOOCC.........",
            "...DDOOOOOOOOOOOOOOOO...........",
            "......DDDDDDDDDDDDDDDD.........."
        ]);
}
