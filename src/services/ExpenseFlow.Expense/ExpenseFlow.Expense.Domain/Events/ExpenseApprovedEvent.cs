using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Domain.Events;

/// <summary>
/// Raised when an expense is approved by a manager.
/// Can trigger reimbursement processing in a downstream finance service.
/// </summary>
public sealed record ExpenseApprovedEvent(
    Guid     ExpenseId,
    Guid     ApprovedByUserId,
    DateTime ApprovedAt) : IDomainEvent;
