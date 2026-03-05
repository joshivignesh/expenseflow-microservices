namespace ExpenseFlow.Expense.Domain.Exceptions;

/// <summary>
/// Thrown when a state transition is attempted that violates the
/// Expense lifecycle rules (e.g. approving a Draft expense directly,
/// or submitting an already-approved expense).
/// Mapped to HTTP 422 Unprocessable Entity by the global exception middleware.
/// </summary>
public sealed class InvalidExpenseStateException : Exception
{
    public InvalidExpenseStateException(string currentState, string attemptedAction)
        : base($"Cannot '{attemptedAction}' an expense that is currently in '{currentState}' state.") { }
}
