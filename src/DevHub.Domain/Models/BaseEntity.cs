namespace DevHub.Domain;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; internal set; } = DateTime.UtcNow;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void BumpUpdated() => UpdatedAt = DateTime.UtcNow;

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool Equals(BaseEntity? other)
        => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object? obj)
        => obj is BaseEntity other && Equals(other);
}
