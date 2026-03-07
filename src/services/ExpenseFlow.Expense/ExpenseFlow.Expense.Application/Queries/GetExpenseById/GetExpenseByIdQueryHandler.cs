using System.Data;
using Dapper;
using MediatR;
using ExpenseFlow.Expense.Application.DTOs;

namespace ExpenseFlow.Expense.Application.Queries.GetExpenseById;

/// <summary>
/// Reads a single expense using raw Dapper SQL.
/// Bypasses EF Core and change tracking for pure read performance.
/// Maps directly to the flat ExpenseDto projection — no domain object needed.
/// </summary>
public sealed class GetExpenseByIdQueryHandler
    : IRequestHandler<GetExpenseByIdQuery, ExpenseDto?>
{
    private readonly IDbConnection _db;

    public GetExpenseByIdQueryHandler(IDbConnection db) => _db = db;

    public async Task<ExpenseDto?> Handle(
        GetExpenseByIdQuery query, CancellationToken ct)
    {
        const string sql = """
            SELECT
                Id,
                SubmittedByUserId,
                Description,
                Amount_Value      AS Amount,
                Amount_Currency   AS Currency,
                Category,
                Status,
                ExpenseDate,
                CreatedAt,
                SubmittedAt,
                ReviewedAt,
                ReviewedByUserId,
                RejectionReason
            FROM Expenses
            WHERE Id = @ExpenseId
            """;

        return await _db.QuerySingleOrDefaultAsync<ExpenseDto>(
            sql, new { query.ExpenseId });
    }
}
