using ExpenseFlow.Shared.Domain.Events;

namespace ExpenseFlow.Identity.Domain.Events;

public sealed class UserDeactivatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string Reason { get; }

    public UserDeactivatedEvent(Guid userId, string email, string reason)
    { UserId = userId; Email = email; Reason = reason; }
}
