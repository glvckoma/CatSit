namespace CatSit.Core.Sprites;

public static class CatSprites
{
    public const int FrameWidth = 40;
    public const int FrameHeight = 30;

    public static readonly SpriteFrame[] Walk = [WalkFrame(0), WalkFrame(1), WalkFrame(2), WalkFrame(3)];
    public static readonly SpriteFrame SittingDown = SittingDownFrame();
    public static readonly SpriteFrame Sitting = SittingFrame();
    public static readonly SpriteFrame Laying = LayingFrame(false, false);
    public static readonly SpriteFrame[] Sleep = [LayingFrame(true, false), LayingFrame(true, true)];

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

    public static IEnumerable<SpriteFrame> AllCatFrames => [.. Walk, SittingDown, Sitting, Laying, .. Sleep];

    private static string[] Head(bool asleep) =>
    [
        "......................O..........O",
        ".....................OPO........OPO",
        "....................OOPOO......OOPOO",
        "....................OOOOOOOOOOOOOOOO",
        "....................OOOODOODDOODOOOO",
        "....................OOODOOOODOOODOOO",
        "....................OOOOOOOOOOOOOOOO",
        asleep ? "....................OOOOOOOOOOOOOOOO" : "....................OOOIIIOOOOIIIOOO",
        asleep ? "....................OOOEEEOOOOEEEOOO" : "....................OOOIEWOOOOIEWOOO",
        asleep ? "....................OOOOOOOOOOOOOOOO" : "....................OOOIIIOOOOIIIOOO",
        "....................SSSOCCCPPCCCOSSS",
        "....................OOOOCCCEECCCOOOO",
        "....................SSSOCCCCCCCCOSSS",
        "....................OOOOOCCCCCCOOOOO",
        ".....................OOOOOOOOOOOOOO"
    ];

    private static SpriteFrame SittingFrame() => Compose(
        Offset(20, [
            "....LLL",
            "...LLDL",
            "...LLLL",
            "....LLL"
        ]),
        Offset(15, [
            ".............OOOOOOOOOOOOOOOOOOO",
            "...........OOOOOOOOOOOOOOOOOOOOO",
            ".........OOOODOOODOOOOOOOOLCCCCLO",
            "........OOOODOOODOOODOOOOOLCCCCCO",
            ".......OOOODOOODOOODOOOOOOLCCCCCO",
            ".......OOOOOOOOOOOOOOOOOOOLCCCCCO",
            "......OOOODOOODOOODOOOOOOOLCCCCCO",
            "......OOOOOOOOOOOOOOOOOOOOLCCCCCO",
            "......OOOODOOODOOODOOOOOOOLCCCCCO",
            "......OOOOOOOOOOOOOOOOOOOOLCCCCCO",
            "......OOOOOOOOOOOOOOOOOOOOCCCDCCO",
            ".......OOOOOOOOOOOOOOOOOOOCCCDCCO",
            "....LLLLDLLLLDLLLLDLLLLLLOCCCDCCO"
        ]),
        Offset(0, Head(false)));

    private static SpriteFrame SittingDownFrame() => Compose(
        Offset(24, [
            "LLLLLL",
            ".LLLLLLL",
            "...LLLLL"
        ]),
        Offset(15, [
            "....................OOOOOOOO",
            "..............OOOOOOOOOOOOOO",
            "..........OOOOOOOOOOOOLCCCLO",
            "........OOOODOOODOOOOOLCCCCO",
            ".......OOOODOOODOOODOOLCCCCO",
            "......OOOOOOOOOOOOOOOOLCCCCO",
            "......OOOODOOODOOODOOOCCCCCO",
            "......OOOOOOOOOOOOOOOOCCCCCO",
            "......OOOOOOOOOOOODDDDOOOOOO",
            ".......OOOOOOOOOOODDDD.OOOO",
            "........OOOOOOOOOODDDD.OOOO",
            ".........OOOOOOOOODDDD.OOOO",
            "..........LLLLLLLODDDD.LLLL"
        ]),
        Offset(1, Head(false)));

    private static SpriteFrame LayingFrame(bool asleep, bool breathing) => Compose(
        Offset(breathing ? 18 : 17, [
            "..........OOOOOOOOOOOOOOOOOOO",
            ".......OOOOOOOOOOOOOOOOOOOOOO",
            ".....OOOOOOOOOOOOOOOOOOOOOOOO",
            "....OOOODOOODOOOODOOOOOOOOOOO",
            "....OOOOODOOODOOOODOOOOOOOOOO",
            "...OOOOOOOOOOOOOOOOOOOOOOOOOO",
            "...OOOOOOOOOOOOOOOOOOOOOOOOOO",
            "...OOOOOOOOOOOOOOOOOOOOOCCCCC",
            "...LLLLDLLLLDLLLLDLLLLOOOCCCCC",
            "....OOOOOOOOOOOOOOOOOOOOOOOO"
        ]),
        Offset(asleep ? 6 : 5, Head(asleep)));

    private static SpriteFrame WalkFrame(int step)
    {
        var contact = step % 2 == 0;
        var bob = contact ? 1 : 0;
        string[] legs = step switch
        {
            0 =>
            [
                "....DDDD.OOOO...DDDD.OOOO",
                "....DDDD.OOOO...DDDD.OOOO",
                "....DDDD.OOOO...DDDD.OOOO",
                "....DDDD.OOOO...DDDD.OOOO",
                "....DDDD.OOOO...DDDD.OOOO",
                "....DDDD.LLLL...DDDD.LLLL"
            ],
            2 =>
            [
                "....OOOO.DDDD...OOOO.DDDD",
                "....OOOO.DDDD...OOOO.DDDD",
                "....OOOO.DDDD...OOOO.DDDD",
                "....OOOO.DDDD...OOOO.DDDD",
                "....OOOO.DDDD...OOOO.DDDD",
                "....LLLL.DDDD...LLLL.DDDD"
            ],
            1 =>
            [
                "...DDDD.OOOO....OOOODDD",
                "...DDDD.OOOO....OOOODDD",
                "...DDDD.OOOO....OOOODDD",
                "...DDDD.OOOO....OOOODDD",
                "....DDDDOOOO.....OOOODD",
                "........OOOO.......DDDD",
                "........LLLL.......DDDD"
            ],
            _ =>
            [
                "...OOOO.DDDD....DDDOOOO",
                "...OOOO.DDDD....DDDOOOO",
                "...OOOO.DDDD....DDDOOOO",
                "...OOOO.DDDD....DDDOOOO",
                "....OOOODDDD.....DDOOOO",
                "........DDDD.......OOOO",
                "........DDDD.......LLLL"
            ]
        };
        return Compose(
            Offset(7 + bob, [
                ".LLL",
                "LLL",
                "LDL",
                "LLL",
                ".LLL",
                ".LDL",
                "..LLL",
                "..LLL",
                "...LL"
            ]),
            Offset(13 + bob, [
                "..........OOOOOOOOOOOOOOO",
                "......OOOOOOOOOOOOOOOOOOO",
                "....OOOOOOOOOOOOOOOOOOOOO",
                "...OOOOOOOOOOOOOOOOOOOOOOO",
                "..OOOODOOODOOOODOOOOOOOOOO",
                "..OOOOODOOODOOOODOOOOOOOOO",
                "..OOOOOOOOOOOOOOOOOOOOOOOO",
                "..OOOOOOOLLLLLLLLOOOOOOOOO",
                "..OOOOOOOOCCCCCCOOOOOOOOOO",
                "...OOOOOOOOOOOOOOOOOOOOOOO"
            ]),
            Offset(22 + bob, legs),
            Offset(bob, Head(false)));
    }

    private static string[] Offset(int rows, string[] art) => [.. Enumerable.Repeat(string.Empty, rows), .. art];

    private static SpriteFrame Compose(params string[][] layers)
    {
        var canvas = new string[FrameHeight];
        Array.Fill(canvas, string.Empty);
        foreach (var layer in layers)
        {
            for (var y = 0; y < layer.Length && y < FrameHeight; y++)
            {
                canvas[y] = Merge(canvas[y], layer[y]);
            }
        }
        for (var y = 0; y < FrameHeight; y++)
        {
            canvas[y] = canvas[y].Length >= FrameWidth
                ? canvas[y][..FrameWidth]
                : canvas[y].PadRight(FrameWidth, '.');
        }
        return new SpriteFrame(canvas);
    }

    private static string Merge(string below, string above)
    {
        var length = Math.Max(below.Length, above.Length);
        var merged = new char[length];
        for (var i = 0; i < length; i++)
        {
            var top = i < above.Length ? above[i] : '.';
            merged[i] = top != '.' ? top : i < below.Length ? below[i] : '.';
        }
        return new string(merged);
    }
}
