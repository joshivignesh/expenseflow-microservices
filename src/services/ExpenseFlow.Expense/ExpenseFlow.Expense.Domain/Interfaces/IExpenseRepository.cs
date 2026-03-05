using ExpenseFlow.Shared.Domain;
using ExpenseFlow.Expense.Domain.Entities;
using ExpenseFlow.Expense.Domain.Enums;

namespace ExpenseFlow.Expense.Domain.Interfaces;

/// <summary>
/// Repository contract for the Expense aggregate.
/// Defined in Domain — implemented in Infrastructure.
/// Follows the same Unit of Work pattern as IUserRepository.
/// </summary>
public interface IExpenseRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Expense>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Expense>> GetByStatusAsync(
        ExpenseStatus status, CancellationToken ct = default);

    Task AddAsync(Expense expense, CancellationToken ct = default);

    void Update(Expense expense);
}
