using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Expense.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Expense.IntegrationTests.Tests;

/// <summary>
/// Integration tests covering the full expense lifecycle:
///   Create → Submit → Approve / Reject
///
/// Model usage:
///   Haiku  — single-step assertions (create, get) U0001f493
///   Sonnet — multi-step flows with routing decisions U0001f9e0
///   Opus   — full lifecycle scenario (create→submit→approve) U0001f3cb️
/// </summary>
[Collection("ExpenseApi")]
public sealed class ExpenseLifecycleTests
{
    private readonly ExpenseApiFactory _factory;

    public ExpenseLifecycleTests(ExpenseApiFactory factory)
    {
        _factory = factory;
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]  // Haiku — simple create assertion U0001f493
    public async Task CreateExpense_ValidRequest_Returns201WithExpenseId()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Flight to Delhi",
            amount      = 5500.00m,
            currency    = "INR",
            category    = 1,  // Travel
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("expenseId");
        body.Should().Contain("Draft");
    }

    [Fact]  // Haiku — simple validation check U0001f493
    public async Task CreateExpense_NegativeAmount_Returns400()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Invalid",
            amount      = -100m,
            currency    = "INR",
            category    = 1,
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]  // Haiku — no token check U0001f493
    public async Task CreateExpense_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/expenses", new
        {
            description = "No auth",
            amount = 100m, currency = "INR",
            category = 1, expenseDate = DateTime.UtcNow.AddDays(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Submit ─────────────────────────────────────────────────────────────

    [Fact]  // Sonnet — two-step flow: create then submit U0001f9e0
    public async Task SubmitExpense_ByOwner_Returns204AndStatusIsSubmitted()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(userId);

        // Create
        var createResponse = await client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Hotel stay",
            amount      = 3200m,
            currency    = "INR",
            category    = 2,  // Accommodation
            expenseDate = DateTime.UtcNow.AddDays(-2)
        });
        var createBody = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();

        // Submit
        var submitResponse = await client.PostAsync(
            $"/api/expenses/{createBody!.ExpenseId}/submit", null);

        submitResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await client.GetAsync(
            $"/api/expenses/{createBody.ExpenseId}");
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Submitted");
    }

    [Fact]  // Sonnet — ownership enforcement U0001f9e0
    public async Task SubmitExpense_ByDifferentUser_Returns403()
    {
        var ownerId     = Guid.NewGuid();
        var intruderId  = Guid.NewGuid();

        var ownerClient   = _factory.CreateAuthenticatedClient(ownerId);
        var intruderClient = _factory.CreateAuthenticatedClient(intruderId);

        // Owner creates
        var createResponse = await ownerClient.PostAsJsonAsync("/api/expenses", new
        {
            description = "Laptop",
            amount      = 75000m,
            currency    = "INR",
            category    = 4,  // Office
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });
        var createBody = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();

        // Intruder tries to submit
        var submitResponse = await intruderClient.PostAsync(
            $"/api/expenses/{createBody!.ExpenseId}/submit", null);

        submitResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Full Lifecycle ────────────────────────────────────────────────────────

    [Fact]  // Opus — full create→submit→approve lifecycle U0001f3cb️
    public async Task FullLifecycle_CreateSubmitApprove_ExpenseIsApproved()
    {
        var employeeId = Guid.NewGuid();
        var managerId  = Guid.NewGuid();

        var employee = _factory.CreateAuthenticatedClient(employeeId, "Employee");
        var manager  = _factory.CreateAuthenticatedClient(managerId,  "Manager");

        // Step 1: Employee creates
        var createResponse = await employee.PostAsJsonAsync("/api/expenses", new
        {
            description = "Team offsite meals",
            amount      = 12500m,
            currency    = "INR",
            category    = 3,  // Meals
            expenseDate = DateTime.UtcNow.AddDays(-3)
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();

        // Step 2: Employee submits
        var submitResponse = await employee.PostAsync(
            $"/api/expenses/{createBody!.ExpenseId}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3: Manager approves
        var approveResponse = await manager.PostAsync(
            $"/api/expenses/{createBody.ExpenseId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify final state
        var getResponse = await employee.GetAsync(
            $"/api/expenses/{createBody.ExpenseId}");
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Approved");
    }

    [Fact]  // Opus — full create→submit→reject lifecycle U0001f3cb️
    public async Task FullLifecycle_CreateSubmitReject_ExpenseIsRejected()
    {
        var employeeId = Guid.NewGuid();
        var managerId  = Guid.NewGuid();

        var employee = _factory.CreateAuthenticatedClient(employeeId, "Employee");
        var manager  = _factory.CreateAuthenticatedClient(managerId,  "Manager");

        // Create + Submit
        var createResponse = await employee.PostAsJsonAsync("/api/expenses", new
        {
            description = "Expensive dinner",
            amount      = 50000m,
            currency    = "INR",
            category    = 3,
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });
        var createBody = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();

        await employee.PostAsync(
            $"/api/expenses/{createBody!.ExpenseId}/submit", null);

        // Manager rejects with reason
        var rejectResponse = await manager.PostAsJsonAsync(
            $"/api/expenses/{createBody.ExpenseId}/reject", new
            {
                reason = "Exceeds meal policy limit of ₹2000 per person."
            });
        rejectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify rejected state
        var getResponse = await employee.GetAsync(
            $"/api/expenses/{createBody.ExpenseId}");
        var body = await getResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Rejected");
        body.Should().Contain("Exceeds meal policy");
    }

    [Fact]  // Sonnet — state machine guard U0001f9e0
    public async Task ApproveExpense_FromDraftState_Returns422()
    {
        var employeeId = Guid.NewGuid();
        var managerId  = Guid.NewGuid();

        var employee = _factory.CreateAuthenticatedClient(employeeId);
        var manager  = _factory.CreateAuthenticatedClient(managerId, "Manager");

        // Create (stays in Draft)
        var createResponse = await employee.PostAsJsonAsync("/api/expenses", new
        {
            description = "Draft expense",
            amount      = 1000m,
            currency    = "INR",
            category    = 4,
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });
        var createBody = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();

        // Try to approve without submitting first
        var approveResponse = await manager.PostAsync(
            $"/api/expenses/{createBody!.ExpenseId}/approve", null);

        // Domain state machine rejects it — 422
        approveResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // Response shape helpers
    private sealed record CreateExpenseResponse(Guid ExpenseId, string Status, DateTime CreatedAt);
}
