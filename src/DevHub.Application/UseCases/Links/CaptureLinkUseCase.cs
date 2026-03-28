using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Links;

public partial class CaptureLinkUseCase
{
    private readonly ILinkRepository _repository;
    private readonly IClipboardService _clipboardService;
    private static readonly HttpClient _httpClient = new();

    public CaptureLinkUseCase(ILinkRepository repository, IClipboardService clipboardService)
    {
        _repository = repository;
        _clipboardService = clipboardService;
    }

    public async Task<Link?> ExecuteAsync(Guid? projectId = null)
    {
        var text = await _clipboardService.GetTextAsync();

        if (string.IsNullOrWhiteSpace(text) || !IsValidUrl(text))
            return null;

        var url = text.Trim();

        var link = new Link
        {
            Url = url,
            Title = await ExtractTitleAsync(url),
            Type = DetectLinkType(url),
            ProjectId = projectId,
            Tags = [],
            CapturedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(link);
        return link;
    }

    private static bool IsValidUrl(string text)
    {
        return Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static async Task<string> ExtractTitleAsync(string url)
    {
        var lower = url.ToLowerInvariant();

        try
        {
            if (lower.Contains("youtube.com") || lower.Contains("youtu.be"))
                return await ExtractYouTubeTitleAsync(url);

            if (lower.Contains("github.com"))
                return ExtractGitHubTitle(url);
        }
        catch { }

        return url;
    }

    private static async Task<string> ExtractYouTubeTitleAsync(string url)
    {
        var oEmbedUrl = $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(url)}&format=json";
        var response = await _httpClient.GetStringAsync(oEmbedUrl);
        using var doc = JsonDocument.Parse(response);

        if (doc.RootElement.TryGetProperty("title", out var title))
            return title.GetString() ?? url;

        return url;
    }

    private static string ExtractGitHubTitle(string url)
    {
        var match = GitHubRepoRegex().Match(url);
        if (match.Success)
        {
            var owner = match.Groups["owner"].Value;
            var repo = match.Groups["repo"].Value;
            return $"{owner}/{repo}";
        }
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
