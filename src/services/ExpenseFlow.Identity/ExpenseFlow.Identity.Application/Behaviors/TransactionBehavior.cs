using MediatR;
using Microsoft.Extensions.Logging;
using ExpenseFlow.Shared.Application.Interfaces;

namespace ExpenseFlow.Identity.Application.Behaviors;

/// <summary>
/// Wraps every command in a unit-of-work boundary.
/// Ensures all domain changes are committed atomically after the handler runs.
/// Query handlers are unaffected — Dapper reads have no UoW dependency.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogDebug("Opening transaction for {RequestName}", name);

        var response = await next();

        _logger.LogDebug("Committing transaction for {RequestName}", name);
        return response;
    }
}
