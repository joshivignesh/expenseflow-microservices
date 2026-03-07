using System.Data;
using Dapper;
using MediatR;
using ExpenseFlow.Expense.Application.DTOs;

namespace ExpenseFlow.Expense.Application.Queries.GetExpensesByUser;

/// <summary>
/// Dapper query — fetches all expenses for a user ordered by creation date descending.
/// No EF Core, no domain model, no change tracking overhead.
/// </summary>
public sealed class GetExpensesByUserQueryHandler
    : IRequestHandler<GetExpensesByUserQuery, IReadOnlyList<ExpenseDto>>
{
    private readonly IDbConnection _db;

    public GetExpensesByUserQueryHandler(IDbConnection db) => _db = db;

    public async Task<IReadOnlyList<ExpenseDto>> Handle(
        GetExpensesByUserQuery query, CancellationToken ct)
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
            WHERE SubmittedByUserId = @UserId
            ORDER BY CreatedAt DESC
            """;

        var results = await _db.QueryAsync<ExpenseDto>(
            sql, new { query.UserId });

        return results.ToList().AsReadOnly();
    }
}
