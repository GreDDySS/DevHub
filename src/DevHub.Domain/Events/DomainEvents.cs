namespace DevHub.Domain.Events;

public record ProjectCreatedEvent(Guid ProjectId, string Name, string Path) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record ProjectFavoriteToggledEvent(Guid ProjectId, bool IsFavorite) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record ProjectHiddenToggledEvent(Guid ProjectId, bool IsHidden) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record ProjectEditedEvent(Guid ProjectId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record LinkCapturedEvent(Guid LinkId, string Url) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
