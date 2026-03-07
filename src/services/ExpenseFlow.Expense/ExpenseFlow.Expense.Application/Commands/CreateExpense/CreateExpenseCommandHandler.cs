using MediatR;
using ExpenseFlow.Expense.Application.DTOs;
using ExpenseFlow.Expense.Domain.Entities;
using ExpenseFlow.Expense.Domain.Interfaces;
using ExpenseFlow.Expense.Domain.ValueObjects;

namespace ExpenseFlow.Expense.Application.Commands.CreateExpense;

/// <summary>
/// Handles CreateExpenseCommand:
///   1. Constructs Money and ExpenseDescription value objects (domain validation fires here)
///   2. Calls Expense.Create() factory — raises ExpenseCreatedEvent internally
///   3. Persists via repository
///   4. SaveEntitiesAsync commits the DB write and dispatches domain events
/// </summary>
public sealed class CreateExpenseCommandHandler
    : IRequestHandler<CreateExpenseCommand, CreateExpenseResponseDto>
{
    private readonly IExpenseRepository _repository;

    public CreateExpenseCommandHandler(IExpenseRepository repository)
        => _repository = repository;

    public async Task<CreateExpenseResponseDto> Handle(
        CreateExpenseCommand command, CancellationToken ct)
    {
        var money       = Money.Create(command.Amount, command.Currency);
        var description = ExpenseDescription.Create(command.Description);

        var expense = Expense.Create(
            command.SubmittedByUserId,
            description,
            money,
            command.Category,
            command.ExpenseDate);

        await _repository.AddAsync(expense, ct);
        await _repository.UnitOfWork.SaveEntitiesAsync(ct);

        return new CreateExpenseResponseDto(
            expense.Id,
            expense.Status.ToString(),
            expense.CreatedAt);
    }
}
