using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ExpenseFlow.Identity.Application.Behaviors;
using ExpenseFlow.Identity.Application.Commands.RegisterUser;
using ExpenseFlow.Identity.Infrastructure;
using ExpenseFlow.Identity.Infrastructure.Settings;
using ExpenseFlow.Identity.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Infrastructure (DbContext, Repositories, Auth Services, DB Seeder) ────
builder.Services.AddIdentityInfrastructure(
    builder.Configuration.GetConnectionString("IdentityDb")
        ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured."));

// ── 2. MediatR — discovers all handlers in the Application assembly ───────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>());

// Pipeline behaviors — order matters: Logging → Validation → Transaction → Handler
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// ── 3. FluentValidation — auto-discovers all validators in Application layer ──
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

// ── 4. JWT Authentication ─────────────────────────────────────────────────────
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

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
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew                = TimeSpan.Zero  // No grace period — token expiry is exact
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── 5. Swagger with JWT Bearer support ───────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "ExpenseFlow — Identity Service API",
        Version     = "v1",
        Description = "Authentication and user management endpoints for the ExpenseFlow platform."
    });

    // Enable the Authorize button in Swagger UI
    c.AddSecurityDefinition("Bearer", new()
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware pipeline (order is critical) ───────────────────────────────────

// Must be first — catches exceptions from everything below it
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpenseFlow Identity API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root /
    });
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
