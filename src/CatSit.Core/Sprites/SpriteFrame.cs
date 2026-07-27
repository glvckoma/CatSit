namespace CatSit.Core.Sprites;

public sealed record SpriteFrame(string[] Rows)
{
    public int Width => Rows[0].Length;
    public int Height => Rows.Length;
}
