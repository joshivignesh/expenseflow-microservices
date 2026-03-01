using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ExpenseFlow.Identity.Application.Interfaces;
using ExpenseFlow.Identity.Domain.Interfaces;
using ExpenseFlow.Identity.Infrastructure.Persistence;
using ExpenseFlow.Identity.Infrastructure.Repositories;
using ExpenseFlow.Identity.Infrastructure.Services;
using ExpenseFlow.Identity.Infrastructure.Settings;

namespace ExpenseFlow.Identity.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services into the DI container.
/// Called once from the API layer's Program.cs.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<JwtSettings>? configureJwt = null)
    {
        // EF Core — SQL Server
        services.AddDbContext<IdentityDbContext>(opts =>
            opts.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(
                    typeof(IdentityDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Auth services
        services.AddScoped<IPasswordHasher,   PasswordHasherService>();
        services.AddScoped<IJwtTokenService,  JwtTokenService>();

        // Database seeder (runs migrations + seeds admin on startup)
        services.AddHostedService<DatabaseSeeder>();

        return services;
    }
}
