using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Expense.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Expense.IntegrationTests.Tests.ExpensesController;

/// <summary>
/// Integration tests for POST /api/expenses (CreateExpense).
/// </summary>
public sealed class CreateExpenseTests : IntegrationTestBase
{
    public CreateExpenseTests(ExpenseWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_ValidExpense_Returns201WithExpenseId()
    {
        // Arrange
        var userId = NewUserId();
        AuthenticateAs(userId);

        var request = new
        {
            description = "Flight to Delhi",
            amount      = 6500.00m,
            currency    = "INR",
            category    = 1,   // Travel
            expenseDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreateExpenseResponse>();
        body!.ExpenseId.Should().NotBeEmpty();
        body.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Create_Unauthenticated_Returns401()
    {
        // No AuthenticateAs call — no token
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Test", amount = 100m,
            currency = "INR", category = 1,
            expenseDate = DateTime.UtcNow
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_FutureDate_Returns400()
    {
        AuthenticateAs(NewUserId());

        var response = await Client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Future expense",
            amount      = 100m,
            currency    = "INR",
            category    = 1,
            expenseDate = DateTime.UtcNow.AddDays(5)   // validator rejects future dates
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ZeroAmount_Returns400()
    {
        AuthenticateAs(NewUserId());

        var response = await Client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Zero amount",
            amount      = 0m,
            currency    = "INR",
            category    = 1,
            expenseDate = DateTime.UtcNow
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record CreateExpenseResponse(
        Guid ExpenseId, string Status, DateTime CreatedAt);
}
