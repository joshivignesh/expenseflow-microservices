using MediatR;
using ExpenseFlow.Expense.Application.DTOs;

namespace ExpenseFlow.Expense.Application.Queries.GetExpensesByUser;

/// <summary>
/// Returns all expenses submitted by a specific user, newest first.
/// Used for the "My Expenses" view in the frontend.
/// </summary>
public sealed record GetExpensesByUserQuery(Guid UserId) : IRequest<IReadOnlyList<ExpenseDto>>;
