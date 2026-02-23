using ExpenseFlow.Shared.Domain.Events;

namespace ExpenseFlow.Shared.Domain;

/// <summary>
/// Base class for Aggregate Roots — the entry point into a DDD aggregate.
/// Aggregate Roots are responsible for:
///   1. Enforcing all business invariants within the aggregate
///   2. Controlling access to child entities
///   3. Raising domain events when state changes occur
///
/// Pattern: Domain events are collected here and dispatched AFTER
/// the transaction commits, ensuring consistency.
///
/// Source: Microsoft Microservices Guide, Chapter 7.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>
    /// Registers a domain event to be dispatched after the transaction commits.
    /// Called from within domain methods — never from outside the aggregate.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event (e.g., if a later operation supersedes it).
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Called by the infrastructure layer after
    /// events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
