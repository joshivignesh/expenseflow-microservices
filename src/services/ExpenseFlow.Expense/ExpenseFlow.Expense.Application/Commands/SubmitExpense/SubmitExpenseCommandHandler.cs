using MediatR;
using ExpenseFlow.Expense.Domain.Exceptions;
using ExpenseFlow.Expense.Domain.Interfaces;

namespace ExpenseFlow.Expense.Application.Commands.SubmitExpense;

/// <summary>
/// Handles SubmitExpenseCommand:
///   1. Loads the expense — throws ExpenseNotFoundException if missing
///   2. Guards ownership — throws UnauthorizedAccessException if wrong user
///   3. Calls expense.Submit() — domain enforces Draft-only transition
///   4. SaveEntitiesAsync dispatches ExpenseSubmittedEvent
/// </summary>
public sealed class SubmitExpenseCommandHandler : IRequestHandler<SubmitExpenseCommand>
{
    private readonly IExpenseRepository _repository;

    public SubmitExpenseCommandHandler(IExpenseRepository repository)
        => _repository = repository;

    public async Task Handle(SubmitExpenseCommand command, CancellationToken ct)
    {
        var expense = await _repository.GetByIdAsync(command.ExpenseId, ct)
            ?? throw new ExpenseNotFoundException(command.ExpenseId);

        if (expense.SubmittedByUserId != command.RequestingUserId)
            throw new UnauthorizedAccessException(
                "You are not authorised to submit this expense.");

        expense.Submit();

        _repository.Update(expense);
        await _repository.UnitOfWork.SaveEntitiesAsync(ct);
    }
}
