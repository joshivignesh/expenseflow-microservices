using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ExpenseFlow.Expense.Domain.Interfaces;
using ExpenseFlow.Expense.Infrastructure.Persistence;
using ExpenseFlow.Expense.Infrastructure.Repositories;

namespace ExpenseFlow.Expense.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services for the Expense bounded context.
/// Called once from the Expense API's Program.cs:
///   builder.Services.AddExpenseInfrastructure(connectionString);
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddExpenseInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ExpenseDbContext>(opts =>
            opts.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(
                    typeof(ExpenseDbContext).Assembly.FullName)));

        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        services.AddHostedService<ExpenseDatabaseSeeder>();

        return services;
    }
}
