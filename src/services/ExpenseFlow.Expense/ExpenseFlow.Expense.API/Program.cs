using System.Data;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ExpenseFlow.Expense.Application.Behaviors;
using ExpenseFlow.Expense.Application.Commands.CreateExpense;
using ExpenseFlow.Expense.Infrastructure;
using ExpenseFlow.Expense.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Infrastructure (DbContext, Repository, DB Seeder) ──────────────────────
var connectionString = builder.Configuration.GetConnectionString("ExpenseDb")
    ?? throw new InvalidOperationException(
        "Connection string 'ExpenseDb' is not configured.");

builder.Services.AddExpenseInfrastructure(connectionString);

// ── 2. Dapper — raw IDbConnection for query handlers ────────────────────────
// Registered as Transient so each query handler gets a fresh connection.
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(connectionString));

// ── 3. MediatR + pipeline behaviors ──────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateExpenseCommand>());

// Order: Logging → Validation → Transaction → Handler
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// ── 4. FluentValidation ──────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<CreateExpenseCommandValidator>();

// ── 5. JWT Authentication (validates tokens issued by the Identity Service) ───
var jwtSection  = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSection["SecretKey"]   ?? throw new InvalidOperationException("JwtSettings:SecretKey missing.");
var issuer      = jwtSection["Issuer"]      ?? throw new InvalidOperationException("JwtSettings:Issuer missing.");
var audience    = jwtSection["Audience"]    ?? throw new InvalidOperationException("JwtSettings:Audience missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── 6. Swagger with JWT Bearer support ───────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "ExpenseFlow — Expense Service API",
        Version     = "v1",
        Description = "Expense lifecycle endpoints: create, submit, approve, reject, and query."
    });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Paste the JWT token issued by the Identity Service."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id   = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware pipeline ─────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();  // Must be first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpenseFlow Expense API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
