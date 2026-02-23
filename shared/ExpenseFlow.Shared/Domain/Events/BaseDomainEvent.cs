namespace ExpenseFlow.Shared.Domain.Events;

/// <summary>
/// Base implementation for all Domain Events.
/// Provides a unique EventId and timestamp automatically.
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
