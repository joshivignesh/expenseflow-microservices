using System.Text.RegularExpressions;
using ExpenseFlow.Shared.Domain;
using ExpenseFlow.Identity.Domain.Exceptions;

namespace ExpenseFlow.Identity.Domain.ValueObjects;

/// <summary>
/// Email Value Object — immutable, always valid by construction.
/// Format validated on creation. Stored lowercase.
/// Two Emails with the same address are equal regardless of casing.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromSeconds(1));

    public string Value { get; }

    private Email() { Value = string.Empty; } // EF Core
    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException("Email cannot be empty.");
        email = email.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(email))
            throw new InvalidEmailException($"'{email}' is not a valid email address.");
        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}
