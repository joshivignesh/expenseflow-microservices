using ExpenseFlow.Shared.Domain.Exceptions;
namespace ExpenseFlow.Identity.Domain.Exceptions;
public sealed class InvalidPasswordException : DomainException
{
    public InvalidPasswordException() : base("Invalid email or password.") { }
}
