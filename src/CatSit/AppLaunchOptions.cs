namespace CatSit;

public sealed record AppLaunchOptions(bool Background)
{
    public static AppLaunchOptions Parse(IEnumerable<string> arguments) => new(
        arguments.Any(argument => string.Equals(argument, "--background", StringComparison.OrdinalIgnoreCase)));
}
