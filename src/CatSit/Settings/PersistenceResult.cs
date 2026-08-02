namespace CatSit.Settings;

public readonly record struct PersistenceResult(bool Success, string ErrorMessage)
{
    public static PersistenceResult Ok() => new(true, string.Empty);

    public static PersistenceResult Failed(string message) => new(false, message);
}

public sealed record PersistenceLoadResult<T>(T Value, PersistenceResult Status);
