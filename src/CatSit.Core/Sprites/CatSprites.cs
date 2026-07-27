namespace CatSit.Core.Sprites;

public static class CatSprites
{
    public const int FrameWidth = 40;
    public const int FrameHeight = 30;

    public static readonly SpriteFrame[] Walk = [WalkFrame(0), WalkFrame(1), WalkFrame(2), WalkFrame(3)];
    public static readonly SpriteFrame Crouch = CrouchFrame();
    public static readonly SpriteFrame Sitting = SittingFrame();
    public static readonly SpriteFrame Laying = LayingFrame(false, false);
    public static readonly SpriteFrame[] Sleep = [LayingFrame(true, false), LayingFrame(true, true)];
    public static readonly SpriteFrame Face = FaceFrame();

    public static readonly SpriteFrame IconFace = new([
        "...O......O.....",
        "..OPO....OPO....",
        ".OOPPO..OOPPO...",
        ".OOOOOOOOOOOOOO.",
        ".OOODOOOODOOOOO.",
        ".OOOOOOOOOOOOOO.",
        ".OOIIOOOOOOIIOO.",
        ".OOEWOOOOOOEWOO.",
        ".OOOOOOOOOOOOOO.",
        ".SOOOCCPPCCOOOS.",
        ".SOOOCCCCCCOOOS.",
        ".OOOOCCCCCCOOOO.",
        ".OOOOOOOOOOOOOO.",
        "..OOOOOOOOOOOO..",
        "...OOOOOOOOOO...",
        "................"
    ]);

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

    public static IEnumerable<SpriteFrame> AllCatFrames => [.. Walk, Crouch, Sitting, Laying, .. Sleep];

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

    private static SpriteFrame CrouchFrame() => Compose(
        Offset(18, [
            "....LLL",
            "...LLDL",
            "...LLLL"
        ]),
        Offset(16, [
            "..........OOOOOOOOOOOOOOOOOOOO",
            ".......OOOOOOOOOOOOOOOOOOOOOOOO",
            ".....OOOODOOODOOOODOOOOOOLCCCCLO",
            "....OOOODOOODOOOODOOOOOOOLCCCCCO",
            "....OOOOOOOOOOOOOOOOOOOOOLCCCCCO",
            "....OOOOOOOOOOOOOOOOOOOOOCCCCCCO",
            "...LLLLLDLLLLDLLLLDLLLLDLLLLLLLL",
            "....OOOO...OOOO.....OOOO..OOOO"
        ]),
        Offset(3, Head(false)));

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
        var legs = step switch
        {
            0 => WalkLegs(5, 10, 17, 22),
            1 => WalkLegs(7, 9, 19, 21),
            2 => WalkLegs(4, 11, 16, 23),
            _ => WalkLegs(6, 9, 18, 21)
        };
        return Compose(
            Offset(7, [
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
            Offset(13, [
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
            Offset(22, legs),
            Offset(0, Head(false)));
    }

    private static string[] WalkLegs(int backNear, int backFar, int frontNear, int frontFar)
    {
        var legs = new string[7];
        for (var row = 0; row < legs.Length; row++)
        {
            var line = new char[FrameWidth];
            Array.Fill(line, '.');
            var paw = row == legs.Length - 1;
            DrawLeg(line, backFar, 'D');
            DrawLeg(line, frontFar, 'D');
            DrawLeg(line, backNear, paw ? 'L' : 'O');
            DrawLeg(line, frontNear, paw ? 'L' : 'O');
            legs[row] = new string(line);
        }
        return legs;
    }

    private static void DrawLeg(char[] line, int x, char shade)
    {
        for (var i = 0; i < 4; i++)
        {
            if (x + i >= 0 && x + i < line.Length) line[x + i] = shade;
        }
    }

    private static SpriteFrame FaceFrame()
    {
        var head = Head(false);
        var rows = new List<string>();
        foreach (var row in head)
        {
            rows.Add(row.PadRight(FrameWidth, '.').Substring(19, 18));
        }
        return new SpriteFrame([.. rows]);
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
