namespace ExpenseFlow.Expense.Application.DTOs;

/// <summary>
/// Read-side projection returned by all expense queries.
/// Flat record — no domain types exposed outside the Application layer.
/// </summary>
public sealed record ExpenseDto(
    Guid     Id,
    Guid     SubmittedByUserId,
    string   Description,
    decimal  Amount,
    string   Currency,
    string   Category,
    string   Status,
    DateTime ExpenseDate,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    Guid?    ReviewedByUserId,
    string?  RejectionReason);
