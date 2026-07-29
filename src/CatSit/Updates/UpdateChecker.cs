using System.Net.Http;
using System.Text.Json;
using CatSit.Core.Updates;

namespace CatSit.Updates;

public sealed record UpdateCheckOutcome(UpdateAvailability Availability, string LatestTag, string ReleaseHtmlUrl);

public static class UpdateChecker
{
    public const string RepoSlug = "glvckoma/CatSit";

    public static async Task<UpdateCheckOutcome?> CheckLatestAsync(HttpClient client, Version current, string? skippedTag, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{RepoSlug}/releases/latest");
            request.Headers.UserAgent.ParseAdd("CatSit-UpdateChecker");
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("tag_name", out var tagElement)) return null;
            if (!doc.RootElement.TryGetProperty("html_url", out var urlElement)) return null;
            var tag = tagElement.GetString();
            var htmlUrl = urlElement.GetString();
            if (tag is null || htmlUrl is null) return null;
            return new UpdateCheckOutcome(UpdateVersionComparer.Evaluate(current, tag, skippedTag), tag, htmlUrl);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            return null;
        }
    }
}
