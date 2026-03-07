using MediatR;
using ExpenseFlow.Expense.Application.DTOs;

namespace ExpenseFlow.Expense.Application.Queries.GetExpenseById;

/// <summary>
/// Fetches a single expense by ID.
/// Returns null if not found — the controller maps that to 404.
/// </summary>
public sealed record GetExpenseByIdQuery(Guid ExpenseId) : IRequest<ExpenseDto?>;
