using System.Net;
using System.Text.Json;
using FluentValidation;
using ExpenseFlow.Expense.Domain.Exceptions;

namespace ExpenseFlow.Expense.API.Middleware;

/// <summary>
/// Global exception handler for the Expense Service.
/// Sits at the top of the middleware pipeline and maps every domain
/// exception to the correct HTTP status + RFC 7807 problem details body.
///
/// Mapping:
///   ValidationException          → 400 Bad Request
///   ExpenseNotFoundException     → 404 Not Found
///   InvalidExpenseStateException → 422 Unprocessable Entity  ← key difference from Identity
///   UnauthorizedAccessException  → 403 Forbidden
///   Everything else              → 500 Internal Server Error
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception on {Method} {Path}: {Message}",
            context.Request.Method, context.Request.Path, ex.Message);

        var (statusCode, title, errors) = ex switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                ve.Errors.Select(e => e.ErrorMessage).ToArray()),

            ExpenseNotFoundException     => (HttpStatusCode.NotFound,
                                             ex.Message, Array.Empty<string>()),

            // 422 — request was understood but business rules prevent it
            InvalidExpenseStateException => (HttpStatusCode.UnprocessableEntity,
                                             ex.Message, Array.Empty<string>()),

            UnauthorizedAccessException  => (HttpStatusCode.Forbidden,
                                             ex.Message, Array.Empty<string>()),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.",
                  Array.Empty<string>())
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var body = JsonSerializer.Serialize(new
        {
            type    = "https://tools.ietf.org/html/rfc7807",
            title,
            status  = (int)statusCode,
            errors,
            traceId = context.TraceIdentifier
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(body);
    }
}
