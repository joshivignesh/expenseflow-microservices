using MediatR;
using ExpenseFlow.Expense.Application.DTOs;
using ExpenseFlow.Expense.Domain.Enums;

namespace ExpenseFlow.Expense.Application.Commands.CreateExpense;

/// <summary>
/// Creates a new expense in Draft status.
/// The submitting user ID comes from the authenticated JWT claim —
/// not from the request body, so a user cannot create expenses for others.
/// </summary>
public sealed record CreateExpenseCommand(
    Guid            SubmittedByUserId,
    string          Description,
    decimal         Amount,
    string          Currency,
    ExpenseCategory Category,
    DateTime        ExpenseDate) : IRequest<CreateExpenseResponseDto>;
