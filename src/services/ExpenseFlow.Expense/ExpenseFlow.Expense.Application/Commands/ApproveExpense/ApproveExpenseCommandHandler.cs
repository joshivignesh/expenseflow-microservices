using MediatR;
using ExpenseFlow.Expense.Domain.Exceptions;
using ExpenseFlow.Expense.Domain.Interfaces;

namespace ExpenseFlow.Expense.Application.Commands.ApproveExpense;

/// <summary>
/// Handles ApproveExpenseCommand:
///   1. Loads expense — throws ExpenseNotFoundException if missing
///   2. Calls expense.Approve(approverId) — domain enforces Submitted-only transition
///   3. SaveEntitiesAsync dispatches ExpenseApprovedEvent
/// Role check (must be Manager/Admin) is enforced at the API layer via [Authorize(Roles=...)].
/// </summary>
public sealed class ApproveExpenseCommandHandler : IRequestHandler<ApproveExpenseCommand>
{
    private readonly IExpenseRepository _repository;

    public ApproveExpenseCommandHandler(IExpenseRepository repository)
        => _repository = repository;

    public async Task Handle(ApproveExpenseCommand command, CancellationToken ct)
    {
        var expense = await _repository.GetByIdAsync(command.ExpenseId, ct)
            ?? throw new ExpenseNotFoundException(command.ExpenseId);

        expense.Approve(command.ApprovedByUserId);

        _repository.Update(expense);
        await _repository.UnitOfWork.SaveEntitiesAsync(ct);
    }
}
