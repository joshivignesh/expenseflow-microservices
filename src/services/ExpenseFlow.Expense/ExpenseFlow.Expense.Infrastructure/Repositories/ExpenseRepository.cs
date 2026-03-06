using Microsoft.EntityFrameworkCore;
using ExpenseFlow.Expense.Domain.Entities;
using ExpenseFlow.Expense.Domain.Enums;
using ExpenseFlow.Expense.Domain.Interfaces;
using ExpenseFlow.Expense.Infrastructure.Persistence;
using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Expense.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IExpenseRepository.
///
/// Write side (commands): routes through EF Core with full change tracking.
/// Read side (queries):   simple projections use Dapper directly,
///                        bypassing EF Core for better read performance.
///
/// This file handles the write side and domain-model reads only.
/// </summary>
public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext _context;

    public ExpenseRepository(ExpenseDbContext context) => _context = context;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Expense>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _context.Expenses
            .Where(e => e.SubmittedByUserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByStatusAsync(
        ExpenseStatus status, CancellationToken ct = default)
        => await _context.Expenses
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Expense expense, CancellationToken ct = default)
        => await _context.Expenses.AddAsync(expense, ct);

    public void Update(Expense expense)
        => _context.Expenses.Update(expense);
}
