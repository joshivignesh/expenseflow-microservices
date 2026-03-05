namespace ExpenseFlow.Expense.Domain.Exceptions;

/// <summary>
/// Thrown when an expense lookup by ID returns no result.
/// Mapped to HTTP 404 Not Found by the global exception middleware.
/// </summary>
public sealed class ExpenseNotFoundException : Exception
{
    public ExpenseNotFoundException(Guid expenseId)
        : base($"Expense with ID '{expenseId}' was not found.") { }
}
