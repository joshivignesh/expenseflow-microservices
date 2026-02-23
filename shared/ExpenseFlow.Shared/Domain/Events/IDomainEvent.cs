using MediatR;

namespace ExpenseFlow.Shared.Domain.Events;

/// <summary>
/// Marker interface for Domain Events.
/// Domain Events represent something that happened within a bounded context.
/// They are dispatched WITHIN the same transaction, handled by domain event handlers
/// inside the same service.
///
/// IMPORTANT: Domain Events are NOT the same as Integration Events.
/// - Domain Events = internal to a service, same transaction
/// - Integration Events = cross-service, via RabbitMQ, eventual consistency
///
/// We implement INotification so MediatR can dispatch domain events
/// to multiple handlers automatically.
///
/// Source: Microsoft Microservices Guide, Chapter 7.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
