namespace CatSit.Core.Sprites;

public static class ColorMath
{
    public static uint Darken(uint color, double amount) => Scale(color, 1.0 - Math.Clamp(amount, 0, 1));

    public static uint Lighten(uint color, double amount)
    {
        var factor = Math.Clamp(amount, 0, 1);
        return Map(color, channel => channel + (255 - channel) * factor);
    }

    public static double Luminance(uint color)
    {
        var r = (color >> 16) & 0xFF;
        var g = (color >> 8) & 0xFF;
        var b = color & 0xFF;
        return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0;
    }

    private static uint Scale(uint color, double factor) => Map(color, channel => channel * factor);

    private static uint Map(uint color, Func<double, double> transform)
    {
        var a = (color >> 24) & 0xFF;
        var r = Channel(transform((color >> 16) & 0xFF));
        var g = Channel(transform((color >> 8) & 0xFF));
        var b = Channel(transform(color & 0xFF));
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    private static uint Channel(double value) => (uint)Math.Clamp(Math.Round(value), 0, 255);
}
