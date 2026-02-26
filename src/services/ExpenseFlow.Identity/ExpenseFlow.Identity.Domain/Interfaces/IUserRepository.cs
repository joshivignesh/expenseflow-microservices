using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Shared.Application.Interfaces;

namespace ExpenseFlow.Identity.Domain.Interfaces;

/// <summary>
/// Repository interface for the User aggregate.
/// Defined in Domain, implemented in Infrastructure — dependency inversion in action.
/// One repository per aggregate root (not a generic repository).
/// </summary>
public interface IUserRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}
