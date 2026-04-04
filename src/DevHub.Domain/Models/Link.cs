using DevHub.Domain.Enums;

namespace DevHub.Domain.Models;

public class Link : BaseEntity
{
    private const int MaxUrlLength = 2048;
    private const int MaxTitleLength = 500;
    private const int MaxNotesLength = 5000;
    private const int MaxTagsCount = 50;
    private const int MaxTagLength = 50;

    public string Url { get; set; } = string.Empty;

    public string? Title { get; set; }

    public LinkType Type { get; set; }

    public Guid? ProjectId { get; set; }

    public List<string> Tags { get; set; } = [];

    public string? Notes { get; set; }

    public DateTime CapturedAt { get; set; }

    public static Link Create(string url, LinkType type = LinkType.Other)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Link URL is required.");
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new DomainException("Link URL must be a valid HTTP/HTTPS URL.");
        if (url.Length > MaxUrlLength)
            throw new DomainException($"URL cannot exceed {MaxUrlLength} characters.");

        return new Link
        {
            Url = url.Trim(),
            Type = type,
            Tags = [],
            CapturedAt = DateTime.UtcNow
        };
    }

    public void SetTitle(string? title)
    {
        if (title?.Length > MaxTitleLength)
            throw new DomainException($"Title cannot exceed {MaxTitleLength} characters.");
        Title = title?.Trim();
    }

    public void SetNotes(string? notes)
    {
        if (notes?.Length > MaxNotesLength)
            throw new DomainException($"Notes cannot exceed {MaxNotesLength} characters.");
        Notes = notes?.Trim();
    }

    public void SetProjectId(Guid? projectId) => ProjectId = projectId;

    public void SetType(LinkType type) => Type = type;

    public void SetTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        Tags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Where(t => t.Length <= MaxTagLength)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxTagsCount)
            .ToList();
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new DomainException("Tag cannot be empty.");
        if (tag.Length > MaxTagLength)
            throw new DomainException($"Tag cannot exceed {MaxTagLength} characters.");
        if (Tags.Count >= MaxTagsCount)
            throw new DomainException($"Cannot add more than {MaxTagsCount} tags.");

        var trimmed = tag.Trim();
        if (!Tags.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            Tags.Add(trimmed);
    }
}
