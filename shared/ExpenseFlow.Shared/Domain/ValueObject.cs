namespace ExpenseFlow.Shared.Domain;

/// <summary>
/// Base class for Value Objects.
/// Value Objects are defined by their attributes, not by identity.
/// They are always immutable — no setters, no Id.
///
/// Examples: Money, Email, Address, ExpensePeriod
///
/// Two Value Objects are equal if all their components are equal.
/// They should be replaced entirely, never mutated.
///
/// Source: Microsoft Microservices Guide, Chapter 7 — Value Objects.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Subclasses must return all properties that define equality for this Value Object.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    /// <summary>
    /// Creates a shallow copy of the Value Object.
    /// Useful when you need to derive a new VO from an existing one.
    /// </summary>
    public ValueObject GetCopy() => (ValueObject)MemberwiseClone();
}
