using Microsoft.EntityFrameworkCore;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Identity.Domain.Interfaces;
using ExpenseFlow.Identity.Infrastructure.Persistence;
using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Identity.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// All writes go through EF Core (command side).
/// Reads that need the domain object (e.g. login) also use EF Core.
/// Simple read-only projections (GetUserProfile query) bypass this and use Dapper directly.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context) => _context = context;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public void Update(User user)
        => _context.Users.Update(user);

    public void Remove(User user)
        => _context.Users.Remove(user);
}
