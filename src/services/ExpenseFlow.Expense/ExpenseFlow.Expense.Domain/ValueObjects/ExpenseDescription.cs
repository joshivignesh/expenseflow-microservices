namespace ExpenseFlow.Expense.Domain.ValueObjects;

/// <summary>
/// Represents the textual description of an expense.
/// Enforces business rules: not empty, max 500 characters.
/// </summary>
public sealed record ExpenseDescription
{
    public const int MaxLength = 500;

    public string Value { get; }

    private ExpenseDescription(string value) => Value = value;

    public static ExpenseDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Expense description cannot be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException(
                $"Expense description cannot exceed {MaxLength} characters.", nameof(value));

        return new ExpenseDescription(value.Trim());
    }

    public override string ToString() => Value;
}
