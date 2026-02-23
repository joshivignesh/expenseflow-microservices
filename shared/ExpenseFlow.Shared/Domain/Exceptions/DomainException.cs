namespace ExpenseFlow.Shared.Domain.Exceptions;

/// <summary>
/// Base exception for all domain rule violations.
/// Thrown when a business invariant is broken.
///
/// Domain exceptions are caught at the API layer and translated to
/// appropriate HTTP status codes (typically 400 Bad Request or 422 Unprocessable Entity).
///
/// Unlike infrastructure exceptions (network failures, DB errors),
/// domain exceptions are expected, meaningful business errors.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
