using MediatR;

namespace ExpenseFlow.Expense.Application.Commands.ApproveExpense;

/// <summary>
/// Approves a Submitted expense.
/// ApprovedByUserId is the manager/admin taken from the JWT claim.
/// </summary>
public sealed record ApproveExpenseCommand(
    Guid ExpenseId,
    Guid ApprovedByUserId) : IRequest;
