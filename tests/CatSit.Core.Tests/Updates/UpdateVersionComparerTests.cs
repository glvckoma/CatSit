using CatSit.Core.Updates;

namespace CatSit.Core.Tests.Updates;

public class UpdateVersionComparerTests
{
    private static readonly Version Current = new(0, 1, 0);

    [Fact]
    public void Evaluate_NewerTag_ReturnsNewVersionAvailable()
    {
        Assert.Equal(UpdateAvailability.NewVersionAvailable,
            UpdateVersionComparer.Evaluate(Current, "v0.2.0", null));
    }

    [Fact]
    public void Evaluate_EqualTag_ReturnsUpToDate()
    {
        Assert.Equal(UpdateAvailability.UpToDate,
            UpdateVersionComparer.Evaluate(Current, "v0.1.0", null));
    }

    [Fact]
    public void Evaluate_OlderTag_ReturnsUpToDate()
    {
        Assert.Equal(UpdateAvailability.UpToDate,
            UpdateVersionComparer.Evaluate(Current, "v0.0.9", null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-version")]
    [InlineData("banana")]
    public void Evaluate_MalformedOrMissingTag_ReturnsUpToDate(string? tag)
    {
        Assert.Equal(UpdateAvailability.UpToDate,
            UpdateVersionComparer.Evaluate(Current, tag, null));
    }

    [Fact]
    public void Evaluate_TagWithoutVPrefix_StillParses()
    {
        Assert.Equal(UpdateAvailability.NewVersionAvailable,
            UpdateVersionComparer.Evaluate(Current, "0.2.0", null));
    }

    [Fact]
    public void Evaluate_UppercaseVPrefix_StillParses()
    {
        Assert.Equal(UpdateAvailability.NewVersionAvailable,
            UpdateVersionComparer.Evaluate(Current, "V0.2.0", null));
    }

    [Fact]
    public void Evaluate_SkippedTagEqualToLatest_ReturnsNewVersionSkipped()
    {
        Assert.Equal(UpdateAvailability.NewVersionSkipped,
            UpdateVersionComparer.Evaluate(Current, "v0.2.0", "v0.2.0"));
    }

    [Fact]
    public void Evaluate_SkipIsVersionSpecific_NotForever()
    {
        Assert.Equal(UpdateAvailability.NewVersionAvailable,
            UpdateVersionComparer.Evaluate(Current, "v0.3.0", "v0.2.0"));
    }

    [Fact]
    public void Evaluate_NullSkippedTag_IsTreatedAsNothingSkipped()
    {
        Assert.Equal(UpdateAvailability.NewVersionAvailable,
            UpdateVersionComparer.Evaluate(Current, "v0.2.0", null));
    }

    [Theory]
    [InlineData("v1.2.3", true, "1.2.3")]
    [InlineData("1.2.3", true, "1.2.3")]
    [InlineData("V1.2.3", true, "1.2.3")]
    [InlineData("", false, "0.0.0.0")]
    [InlineData(null, false, "0.0.0.0")]
    [InlineData("garbage", false, "0.0.0.0")]
    public void TryParseVersionTag_ParsesOrFailsAsExpected(string? tag, bool expectedSuccess, string expectedVersion)
    {
        var success = UpdateVersionComparer.TryParseVersionTag(tag, out var version);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(Version.Parse(expectedVersion), version);
    }
}
