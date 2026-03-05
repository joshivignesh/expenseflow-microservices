using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Domain.Events;

/// <summary>
/// Raised when an expense is rejected by a manager.
/// The rejection reason is included so notification services
/// can inform the submitter why their claim was denied.
/// </summary>
public sealed record ExpenseRejectedEvent(
    Guid     ExpenseId,
    Guid     RejectedByUserId,
    string   Reason,
    DateTime RejectedAt) : IDomainEvent;
