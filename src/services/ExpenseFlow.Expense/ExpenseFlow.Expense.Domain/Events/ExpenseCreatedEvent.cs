using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Domain.Events;

/// <summary>
/// Raised when a new expense is created in Draft status.
/// Downstream consumers (e.g. notification service) can listen to this
/// via the message broker to send confirmation to the submitter.
/// </summary>
public sealed record ExpenseCreatedEvent(
    Guid   ExpenseId,
    Guid   SubmittedByUserId,
    string Description,
    decimal Amount,
    string Currency,
    DateTime CreatedAt) : IDomainEvent;
