using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Domain.Events;

/// <summary>
/// Raised when an expense moves from Draft → Submitted.
/// Triggers the approval workflow — e.g. notifies the approver.
/// </summary>
public sealed record ExpenseSubmittedEvent(
    Guid   ExpenseId,
    Guid   SubmittedByUserId,
    decimal Amount,
    string Currency,
    DateTime SubmittedAt) : IDomainEvent;
