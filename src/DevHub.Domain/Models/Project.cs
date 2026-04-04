using DevHub.Domain.Enums;
using DevHub.Domain.Events;

namespace DevHub.Domain.Models;

public class Project : BaseEntity
{
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 1000;
    private const int MaxNotesLength = 5000;
    private const int MaxTagsCount = 50;
    private const int MaxTagLength = 50;

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public ProgrammingLanguage Language { get; set; }

    public ProjectStatus Status { get; set; }

    public List<string> Tags { get; set; } = [];

    public string? PreferredIde { get; set; }

    public bool IsFavorite { get; set; }

    public bool IsHidden { get; set; }

    public DateTime? LastAccessedAt { get; set; }

    public bool AutoStatusEnabled { get; set; } = true;

    public static Project Create(string name, string path, ProgrammingLanguage language)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");
        if (name.Length > MaxNameLength)
            throw new DomainException($"Project name cannot exceed {MaxNameLength} characters.");
        if (string.IsNullOrWhiteSpace(path))
            throw new DomainException("Project path is required.");

        return new Project
        {
            Name = name.Trim(),
            Path = path.Trim(),
            Language = language,
            Status = ProjectStatus.Active,
            Tags = [],
            AutoStatusEnabled = true
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");
        if (name.Length > MaxNameLength)
            throw new DomainException($"Project name cannot exceed {MaxNameLength} characters.");

        Name = name.Trim();
        BumpUpdated();
    }

    public void UpdateDescription(string? description)
    {
        if (description?.Length > MaxDescriptionLength)
            throw new DomainException($"Description cannot exceed {MaxDescriptionLength} characters.");

        Description = description?.Trim();
        BumpUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        if (notes?.Length > MaxNotesLength)
            throw new DomainException($"Notes cannot exceed {MaxNotesLength} characters.");

        Notes = notes?.Trim();
        BumpUpdated();
    }

    public void ChangeStatus(ProjectStatus status)
    {
        Status = status;
        BumpUpdated();
    }

    public void ChangeLanguage(ProgrammingLanguage language)
    {
        Language = language;
        BumpUpdated();
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        AddDomainEvent(new ProjectFavoriteToggledEvent(Id, IsFavorite));
        BumpUpdated();
    }

    public void ToggleHidden()
    {
        IsHidden = !IsHidden;
        AddDomainEvent(new ProjectHiddenToggledEvent(Id, IsHidden));
        BumpUpdated();
    }

    public void SetPreferredIde(string? idePath)
    {
        PreferredIde = idePath;
        BumpUpdated();
    }

    public void SetTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var tagList = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Where(t => t.Length <= MaxTagLength)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxTagsCount)
            .ToList();

        Tags = tagList;
        BumpUpdated();
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
        {
            Tags.Add(trimmed);
            BumpUpdated();
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        BumpUpdated();
    }

    public void MarkAccessed() => LastAccessedAt = DateTime.UtcNow;

    public void EnableAutoStatus() => AutoStatusEnabled = true;

    public void DisableAutoStatus() => AutoStatusEnabled = false;

    public ProjectStatus GetEffectiveStatus(DateTime? lastFileWrite = null)
    {
        if (AutoStatusEnabled && Status == ProjectStatus.Active)
        {
            var checkDate = lastFileWrite ?? UpdatedAt;
            if (DateTime.UtcNow - checkDate > TimeSpan.FromDays(14))
                return ProjectStatus.Paused;
        }
        return Status;
    }
}
