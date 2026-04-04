namespace DevHub.Domain;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
