using Dapper;
using MediatR;
using System.Data;
using ExpenseFlow.Identity.Application.DTOs;

namespace ExpenseFlow.Identity.Application.Queries.GetUserProfile;

/// <summary>
/// Uses Dapper directly against the DB — no domain model, no EF Core overhead.
/// This is the CQRS read side: optimised for fast, thin data projection.
/// </summary>
public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IDbConnection _db;

    public GetUserProfileQueryHandler(IDbConnection db) => _db = db;

    public Task<UserProfileDto?> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email_Value  AS Email,
                u.Role,
                u.Status,
                u.CreatedAt,
                u.LastLoginAt
            FROM Users u
            WHERE u.Id     = @UserId
              AND u.Status = 1";  -- Active only

        return _db.QuerySingleOrDefaultAsync<UserProfileDto>(sql, new { query.UserId });
    }
}
