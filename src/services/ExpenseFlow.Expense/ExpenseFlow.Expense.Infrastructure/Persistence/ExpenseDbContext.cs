using Microsoft.EntityFrameworkCore;
using ExpenseFlow.Expense.Domain.Entities;
using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Expense bounded context.
/// Owns only the Expenses table — no cross-service references.
///
/// SaveEntitiesAsync dispatches domain events (ExpenseCreated, ExpenseApproved etc.)
/// after the DB write succeeds, keeping events and persistence in sync.
/// </summary>
public sealed class ExpenseDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher _dispatcher;

    public ExpenseDbContext(
        DbContextOptions<ExpenseDbContext> options,
        IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Auto-discovers all IEntityTypeConfiguration<T> in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ExpenseDbContext).Assembly);
    }

    /// <summary>
    /// Persists all tracked changes then dispatches domain events.
    /// Always call this instead of SaveChangesAsync in command handlers.
    /// </summary>
    public async Task<bool> SaveEntitiesAsync(CancellationToken ct = default)
    {
        await base.SaveChangesAsync(ct);

        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _dispatcher.DispatchAsync(domainEvent, ct);

        return true;
    }
}
