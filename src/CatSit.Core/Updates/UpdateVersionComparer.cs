namespace CatSit.Core.Updates;

public enum UpdateAvailability
{
    UpToDate,
    NewVersionAvailable,
    NewVersionSkipped
}

public static class UpdateVersionComparer
{
    public static bool TryParseVersionTag(string? tag, out Version version)
    {
        version = new Version(0, 0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag)) return false;
        var trimmed = tag.Trim();
        if (trimmed.Length > 0 && (trimmed[0] == 'v' || trimmed[0] == 'V')) trimmed = trimmed[1..];
        if (!Version.TryParse(trimmed, out var parsed)) return false;
        version = parsed;
        return true;
    }

    public static UpdateAvailability Evaluate(Version current, string? latestTag, string? skippedTag)
    {
        if (!TryParseVersionTag(latestTag, out var latest) || latest <= current)
        {
            return UpdateAvailability.UpToDate;
        }
        if (TryParseVersionTag(skippedTag, out var skipped) && latest <= skipped)
        {
            return UpdateAvailability.NewVersionSkipped;
        }
        return UpdateAvailability.NewVersionAvailable;
    }
}
