using ExpenseFlow.Shared.Domain.Events;
using ExpenseFlow.Identity.Domain.Enums;

namespace ExpenseFlow.Identity.Domain.Events;

public sealed class UserRegisteredEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public UserRole Role { get; }

    public UserRegisteredEvent(Guid userId, string email, string fullName, UserRole role)
    { UserId = userId; Email = email; FullName = fullName; Role = role; }
}
