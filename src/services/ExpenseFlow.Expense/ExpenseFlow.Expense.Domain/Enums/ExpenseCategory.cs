namespace ExpenseFlow.Expense.Domain.Enums;

/// <summary>
/// Business categories for expense classification.
/// Used for reporting, budget tracking, and approval routing.
/// </summary>
public enum ExpenseCategory
{
    Travel        = 1,
    Accommodation = 2,
    Meals         = 3,
    Office        = 4,
    Software      = 5,
    Training      = 6,
    Marketing     = 7,
    Other         = 99
}
