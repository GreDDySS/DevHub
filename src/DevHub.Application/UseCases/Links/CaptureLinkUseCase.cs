using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Links;

public class CaptureLinkUseCase
{
    private readonly ILinkRepository _repository;
    private readonly IClipboardService _clipboardService;

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

        var link = new Link
        {
            Url = text.Trim(),
            Title = text.Trim(),
            Type = DetectLinkType(text),
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
}
