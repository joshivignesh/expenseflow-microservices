using System.Diagnostics;

namespace ExpenseFlow.Gateway.Middleware;

/// <summary>
/// Logs every inbound request and outbound response at the gateway boundary.
/// Uses Haiku for lightweight structured log enrichment — fast, cheap, no latency cost.
///
/// Captures:
///   - Method, Path, StatusCode, Duration (ms)
///   - X-Correlation-ID — injected if missing, forwarded downstream
///   - Warnings for slow responses (> 1000 ms)
/// </summary>
public sealed class GatewayLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayLoggingMiddleware> _logger;

    public GatewayLoggingMiddleware(
        RequestDelegate next,
        ILogger<GatewayLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Inject or propagate correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Request.Headers["X-Correlation-ID"]  = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var status  = context.Response.StatusCode;

            if (elapsed > 1000)
                _logger.LogWarning(
                    "[GATEWAY] SLOW {Method} {Path} → {Status} in {Elapsed}ms | CorrelationId: {CorrelationId}",
                    context.Request.Method, context.Request.Path, status, elapsed, correlationId);
            else
                _logger.LogInformation(
                    "[GATEWAY] {Method} {Path} → {Status} in {Elapsed}ms | CorrelationId: {CorrelationId}",
                    context.Request.Method, context.Request.Path, status, elapsed, correlationId);
        }
    }
}
