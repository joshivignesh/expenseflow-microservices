using System.Net;
using System.Text.Json;
using FluentValidation;
using ExpenseFlow.Identity.Domain.Exceptions;

namespace ExpenseFlow.Identity.API.Middleware;

/// <summary>
/// Global exception handler — sits at the top of the middleware pipeline.
/// Catches all unhandled exceptions and maps them to RFC 7807 problem details JSON.
/// Keeps controllers clean: they never need try/catch blocks.
///
/// Mapping:
///   ValidationException        → 400 Bad Request  (list of field errors)
///   DuplicateEmailException    → 409 Conflict
///   UserNotFoundException      → 404 Not Found
///   InvalidPasswordException   → 401 Unauthorized
///   InvalidEmailException      → 400 Bad Request
///   Everything else            → 500 Internal Server Error
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

            DuplicateEmailException  => (HttpStatusCode.Conflict,      ex.Message, Array.Empty<string>()),
            UserNotFoundException    => (HttpStatusCode.NotFound,       ex.Message, Array.Empty<string>()),
            InvalidPasswordException => (HttpStatusCode.Unauthorized,   "Invalid credentials.", Array.Empty<string>()),
            InvalidEmailException    => (HttpStatusCode.BadRequest,     ex.Message, Array.Empty<string>()),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.",
                  Array.Empty<string>())
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type   = $"https://tools.ietf.org/html/rfc7807",
            title,
            status = (int)statusCode,
            errors,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
