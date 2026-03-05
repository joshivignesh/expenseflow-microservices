using ExpenseFlow.Shared.Domain;
using ExpenseFlow.Expense.Domain.Enums;
using ExpenseFlow.Expense.Domain.Events;
using ExpenseFlow.Expense.Domain.Exceptions;
using ExpenseFlow.Expense.Domain.ValueObjects;

namespace ExpenseFlow.Expense.Domain.Entities;

/// <summary>
/// The core aggregate root of the Expense bounded context.
///
/// Enforces the expense lifecycle state machine:
///   Draft → Submit() → Submitted
///   Submitted → Approve(approverId) → Approved
///   Submitted → Reject(approverId, reason) → Rejected
///
/// All state changes raise domain events that are dispatched after
/// SaveEntitiesAsync — downstream services react without tight coupling.
/// </summary>
public sealed class Expense : AggregateRoot
{
    public Guid               SubmittedByUserId { get; private set; }
    public ExpenseDescription Description       { get; private set; } = default!;
    public Money              Amount            { get; private set; } = default!;
    public ExpenseCategory    Category          { get; private set; }
    public ExpenseStatus      Status            { get; private set; }
    public DateTime           ExpenseDate       { get; private set; }
    public DateTime           CreatedAt         { get; private set; }
    public DateTime?          SubmittedAt       { get; private set; }
    public DateTime?          ReviewedAt        { get; private set; }
    public Guid?              ReviewedByUserId  { get; private set; }
    public string?            RejectionReason   { get; private set; }

    // EF Core requires a parameterless constructor
    private Expense() { }

    /// <summary>
    /// Factory method — the only way to create a valid Expense.
    /// Starts in Draft status and raises ExpenseCreatedEvent.
    /// </summary>
    public static Expense Create(
        Guid               submittedByUserId,
        ExpenseDescription description,
        Money              amount,
        ExpenseCategory    category,
        DateTime           expenseDate)
    {
        var expense = new Expense
        {
            Id                = Guid.NewGuid(),
            SubmittedByUserId = submittedByUserId,
            Description       = description,
            Amount            = amount,
            Category          = category,
            Status            = ExpenseStatus.Draft,
            ExpenseDate       = expenseDate,
            CreatedAt         = DateTime.UtcNow
        };

        expense.AddDomainEvent(new ExpenseCreatedEvent(
            expense.Id,
            submittedByUserId,
            description.Value,
            amount.Amount,
            amount.Currency,
            expense.CreatedAt));

        return expense;
    }

    /// <summary>
    /// Transitions Draft → Submitted.
    /// Only a Draft expense can be submitted.
    /// </summary>
    public void Submit()
    {
        if (Status != ExpenseStatus.Draft)
            throw new InvalidExpenseStateException(Status.ToString(), nameof(Submit));

        Status      = ExpenseStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;

        AddDomainEvent(new ExpenseSubmittedEvent(
            Id, SubmittedByUserId,
            Amount.Amount, Amount.Currency,
            SubmittedAt.Value));
    }

    /// <summary>
    /// Transitions Submitted → Approved.
    /// Only a manager/admin can approve; only Submitted expenses are eligible.
    /// </summary>
    public void Approve(Guid approvedByUserId)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidExpenseStateException(Status.ToString(), nameof(Approve));

        Status           = ExpenseStatus.Approved;
        ReviewedAt       = DateTime.UtcNow;
        ReviewedByUserId = approvedByUserId;

        AddDomainEvent(new ExpenseApprovedEvent(
            Id, approvedByUserId, ReviewedAt.Value));
    }

    /// <summary>
    /// Transitions Submitted → Rejected.
    /// Rejection reason is mandatory — the submitter must know why.
    /// </summary>
    public void Reject(Guid rejectedByUserId, string reason)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidExpenseStateException(Status.ToString(), nameof(Reject));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason must be provided.", nameof(reason));

        Status           = ExpenseStatus.Rejected;
        ReviewedAt       = DateTime.UtcNow;
        ReviewedByUserId = rejectedByUserId;
        RejectionReason  = reason;

        AddDomainEvent(new ExpenseRejectedEvent(
            Id, rejectedByUserId, reason, ReviewedAt.Value));
    }
}
