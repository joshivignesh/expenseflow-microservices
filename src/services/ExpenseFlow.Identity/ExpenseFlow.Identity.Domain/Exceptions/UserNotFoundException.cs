using ExpenseFlow.Shared.Domain.Exceptions;
namespace ExpenseFlow.Identity.Domain.Exceptions;
public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid id) : base($"User '{id}' not found.") { }
    public UserNotFoundException(string email) : base($"User with email '{email}' not found.") { }
}
