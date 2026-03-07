using System.Data;
using Dapper;
using MediatR;
using ExpenseFlow.Expense.Application.DTOs;

namespace ExpenseFlow.Expense.Application.Queries.GetExpensesByStatus;

/// <summary>
/// Dapper query — fetches all expenses filtered by status.
/// The Status int value maps directly to the column stored by EF Core.
/// </summary>
public sealed class GetExpensesByStatusQueryHandler
    : IRequestHandler<GetExpensesByStatusQuery, IReadOnlyList<ExpenseDto>>
{
    private readonly IDbConnection _db;

    public GetExpensesByStatusQueryHandler(IDbConnection db) => _db = db;

    public async Task<IReadOnlyList<ExpenseDto>> Handle(
        GetExpensesByStatusQuery query, CancellationToken ct)
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
            WHERE Status = @Status
            ORDER BY CreatedAt DESC
            """;

        var results = await _db.QueryAsync<ExpenseDto>(
            sql, new { Status = (int)query.Status });

        return results.ToList().AsReadOnly();
    }
}
