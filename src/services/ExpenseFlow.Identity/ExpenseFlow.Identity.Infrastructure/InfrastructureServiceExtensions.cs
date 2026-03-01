using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ExpenseFlow.Identity.Domain.Interfaces;
using ExpenseFlow.Identity.Infrastructure.Persistence;
using ExpenseFlow.Identity.Infrastructure.Repositories;

namespace ExpenseFlow.Identity.Infrastructure;

/// <summary>
/// Extension method to wire up all Infrastructure dependencies in one call.
/// Called from the API layer's Program.cs: builder.Services.AddIdentityInfrastructure(config);
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(opts =>
            opts.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
