using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ExpenseFlow.Identity.Application.Behaviors;

/// <summary>
/// Logs every request name and elapsed time.
/// Sits first in the pipeline so it wraps all other behaviors.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", name);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Error handling {RequestName} after {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
