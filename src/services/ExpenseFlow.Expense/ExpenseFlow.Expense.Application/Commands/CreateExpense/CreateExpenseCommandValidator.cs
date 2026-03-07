using FluentValidation;
using ExpenseFlow.Expense.Domain.ValueObjects;

namespace ExpenseFlow.Expense.Application.Commands.CreateExpense;

public sealed class CreateExpenseCommandValidator
    : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.SubmittedByUserId)
            .NotEmpty().WithMessage("SubmittedByUserId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(ExpenseDescription.MaxLength)
            .WithMessage($"Description cannot exceed {ExpenseDescription.MaxLength} characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g. INR, USD).")
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must contain only letters.");

        RuleFor(x => x.ExpenseDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Expense date cannot be in the future.");
    }
}
