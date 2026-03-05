namespace ExpenseFlow.Expense.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of an expense.
/// Transitions are enforced by the Expense aggregate — no direct enum assignment allowed.
///
/// Valid transitions:
///   Draft  → Submitted
///   Submitted → Approved
///   Submitted → Rejected
/// Any other transition throws InvalidExpenseStateException.
/// </summary>
public enum ExpenseStatus
{
    Draft     = 0,
    Submitted = 1,
    Approved  = 2,
    Rejected  = 3
}
