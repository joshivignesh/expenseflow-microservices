using FluentValidation;

namespace ExpenseFlow.Expense.Application.Commands.RejectExpense;

public sealed class RejectExpenseCommandValidator
    : AbstractValidator<RejectExpenseCommand>
{
    public RejectExpenseCommandValidator()
    {
        RuleFor(x => x.ExpenseId)
            .NotEmpty();

        RuleFor(x => x.RejectedByUserId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A rejection reason is required.")
            .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.");
    }
}
