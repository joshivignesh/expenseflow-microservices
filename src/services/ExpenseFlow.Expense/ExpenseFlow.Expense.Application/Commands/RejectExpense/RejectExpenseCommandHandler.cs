using MediatR;
using ExpenseFlow.Expense.Domain.Exceptions;
using ExpenseFlow.Expense.Domain.Interfaces;

namespace ExpenseFlow.Expense.Application.Commands.RejectExpense;

/// <summary>
/// Handles RejectExpenseCommand:
///   1. Loads expense
///   2. Calls expense.Reject(rejectorId, reason) — domain validates Submitted-only + non-empty reason
///   3. SaveEntitiesAsync dispatches ExpenseRejectedEvent
/// </summary>
public sealed class RejectExpenseCommandHandler : IRequestHandler<RejectExpenseCommand>
{
    private readonly IExpenseRepository _repository;

    public RejectExpenseCommandHandler(IExpenseRepository repository)
        => _repository = repository;

    public async Task Handle(RejectExpenseCommand command, CancellationToken ct)
    {
        var expense = await _repository.GetByIdAsync(command.ExpenseId, ct)
            ?? throw new ExpenseNotFoundException(command.ExpenseId);

        expense.Reject(command.RejectedByUserId, command.Reason);

        _repository.Update(expense);
        await _repository.UnitOfWork.SaveEntitiesAsync(ct);
    }
}
