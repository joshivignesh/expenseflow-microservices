using MediatR;

namespace ExpenseFlow.Expense.Application.Commands.SubmitExpense;

/// <summary>
/// Transitions an expense from Draft → Submitted.
/// Only the owner of the expense can submit it —
/// the handler validates RequestingUserId matches SubmittedByUserId.
/// </summary>
public sealed record SubmitExpenseCommand(
    Guid ExpenseId,
    Guid RequestingUserId) : IRequest;
