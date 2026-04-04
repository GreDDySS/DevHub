using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Links;

public partial class CaptureLinkUseCase(ILinkRepository repository, IClipboardService clipboardService, HttpClient httpClient) : ICaptureLinkUseCase
{
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(10);

    public async Task<Link?> ExecuteAsync(Guid? projectId, CancellationToken ct = default)
    {
        var text = await clipboardService.GetTextAsync(ct);

        if (string.IsNullOrWhiteSpace(text) || !IsValidUrl(text))
            return null;

        var url = text.Trim();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(HttpTimeout);

        var link = Link.Create(url, DetectLinkType(url));
        link.SetTitle(await ExtractTitleAsync(url, cts.Token));
        link.SetProjectId(projectId);

        await repository.AddAsync(link, ct);
        return link;
    }

    private static bool IsValidUrl(string text)
        => Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private async Task<string> ExtractTitleAsync(string url, CancellationToken ct)
    {
        var lower = url.ToLowerInvariant();

        try
        {
            if (lower.Contains("youtube.com") || lower.Contains("youtu.be"))
                return await ExtractYouTubeTitleAsync(url, ct);

            if (lower.Contains("github.com"))
                return ExtractGitHubTitle(url);
        }
        catch
        {
            // Network errors are acceptable — fall back to URL
        }

        return url;
    }

    private async Task<string> ExtractYouTubeTitleAsync(string url, CancellationToken ct)
    {
        var oEmbedUrl = $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(url)}&format=json";
        var response = await httpClient.GetStringAsync(oEmbedUrl, ct);
        using var doc = JsonDocument.Parse(response);

        if (doc.RootElement.TryGetProperty("title", out var title))
            return title.GetString() ?? url;

        return url;
    }

    private static string ExtractGitHubTitle(string url)
    {
        var match = GitHubRepoRegex().Match(url);
        if (match.Success)
            return $"{match.Groups["owner"].Value}/{match.Groups["repo"].Value}";

        return url;
    }

    private static LinkType DetectLinkType(string url)
    {
        url = url.ToLowerInvariant();

        if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            return LinkType.YouTube;
        if (url.Contains("github.com") || url.Contains("gitlab.com") || url.Contains("bitbucket.org"))
            return LinkType.Repository;
        if (url.Contains("docs.") || url.Contains("doc.") || url.Contains("/docs/"))
            return LinkType.Documentation;

        return LinkType.Article;
    }

    [GeneratedRegex(@"github\.com/(?<owner>[^/]+)/(?<repo>[^/?#]+)")]
    private static partial Regex GitHubRepoRegex();
}
