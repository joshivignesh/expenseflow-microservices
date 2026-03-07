using MediatR;

namespace ExpenseFlow.Expense.Application.Commands.RejectExpense;

/// <summary>
/// Rejects a Submitted expense with a mandatory reason.
/// </summary>
public sealed record RejectExpenseCommand(
    Guid   ExpenseId,
    Guid   RejectedByUserId,
    string Reason) : IRequest;
