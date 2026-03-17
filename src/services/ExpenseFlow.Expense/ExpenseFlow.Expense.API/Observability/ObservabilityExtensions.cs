using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using ExpenseFlow.Expense.API.HealthChecks;

namespace ExpenseFlow.Expense.API.Observability;

/// <summary>
/// Wires up the full observability stack for the Expense Service:
///   - Serilog         : structured JSON logging → stdout
///   - OpenTelemetry   : distributed tracing (OTLP) + metrics (Prometheus)
///   - Health checks   : /health endpoint polled by YARP and orchestrators
/// </summary>
public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "ExpenseFlow.Expense")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
            .CreateLogger();

        builder.Host.UseSerilog();

        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"]
                           ?? "http://localhost:4317";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService("ExpenseFlow.Expense", serviceVersion: "1.0.0"))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        builder.Services
            .AddHealthChecks()
            .AddCheck<ExpenseHealthCheck>("expense-db");

        return builder;
    }

    public static WebApplication MapObservability(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status  = report.Status.ToString(),
                    service = "ExpenseFlow.Expense",
                    checks  = report.Entries.Select(e => new
                    {
                        name        = e.Key,
                        status      = e.Value.Status.ToString(),
                        description = e.Value.Description
                    })
                });
                await ctx.Response.WriteAsync(result);
            }
        });

        app.MapPrometheusScrapingEndpoint("/metrics");
        return app;
    }
}
