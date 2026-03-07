using MediatR;
using ExpenseFlow.Expense.Application.DTOs;
using ExpenseFlow.Expense.Domain.Enums;

namespace ExpenseFlow.Expense.Application.Queries.GetExpensesByStatus;

/// <summary>
/// Returns all expenses with a given status.
/// Used by managers/admins for the approval queue (Status = Submitted).
/// </summary>
public sealed record GetExpensesByStatusQuery(
    ExpenseStatus Status) : IRequest<IReadOnlyList<ExpenseDto>>;
