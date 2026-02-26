using ExpenseFlow.Shared.Domain.Exceptions;
namespace ExpenseFlow.Identity.Domain.Exceptions;
public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) : base(message) { }
}
