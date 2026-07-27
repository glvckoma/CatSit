namespace CatSit.Core.Detection;

public static class KeyAdjacency
{
    private static readonly Dictionary<int, (int Row, int Col)> Positions = BuildPositions();

    public static bool AreAdjacent(int vk1, int vk2)
    {
        if (vk1 == vk2) return false;
        if (!Positions.TryGetValue(vk1, out var a) || !Positions.TryGetValue(vk2, out var b)) return false;
        return Math.Abs(a.Row - b.Row) <= 1 && Math.Abs(a.Col - b.Col) <= 1;
    }

    private static Dictionary<int, (int, int)> BuildPositions()
    {
        string[] rows = ["1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM"];
        var positions = new Dictionary<int, (int, int)>();
        for (var row = 0; row < rows.Length; row++)
        {
            for (var col = 0; col < rows[row].Length; col++)
            {
                positions[rows[row][col]] = (row, col);
            }
        }
        return positions;
    }
}
