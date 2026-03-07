namespace ExpenseFlow.Expense.Application.DTOs;

/// <summary>
/// Returned by the CreateExpense command handler.
/// Gives the caller the new expense ID immediately so they
/// can redirect to GET /expenses/{id}.
/// </summary>
public sealed record CreateExpenseResponseDto(
    Guid     ExpenseId,
    string   Status,
    DateTime CreatedAt);
