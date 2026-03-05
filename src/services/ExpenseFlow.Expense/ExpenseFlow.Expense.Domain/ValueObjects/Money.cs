namespace ExpenseFlow.Expense.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// Immutable value object — equality is by value, not reference.
///
/// Guards against:
///   - Negative or zero amounts
///   - Invalid currency codes (must be 3 uppercase letters, e.g. INR, USD, GBP)
///   - Accidental cross-currency arithmetic (Add/Subtract enforce same currency)
/// </summary>
public sealed record Money
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount   = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3
            || !currency.All(char.IsLetter))
            throw new ArgumentException(
                "Currency must be a valid 3-letter ISO 4217 code (e.g. INR, USD).",
                nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        if (other.Amount > Amount)
            throw new InvalidOperationException("Cannot subtract a larger amount.");
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} vs {other.Currency}.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
