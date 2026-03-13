using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Expense.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Expense.IntegrationTests.Tests.ExpensesController;

/// <summary>
/// End-to-end lifecycle tests:
///   Draft → Submitted → Approved
///   Draft → Submitted → Rejected
///   Invalid transitions → 422
/// These are the most valuable tests in the suite — they exercise the
/// entire stack including the aggregate state machine.
/// </summary>
public sealed class ExpenseLifecycleTests : IntegrationTestBase
{
    public ExpenseLifecycleTests(ExpenseWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task FullApprovalFlow_DraftToApproved_Succeeds()
    {
        // Arrange
        var employeeId = NewUserId();
        var managerId  = NewUserId();

        // Step 1 — Create (Draft)
        AuthenticateAs(employeeId);
        var createResponse = await Client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Hotel stay",
            amount      = 3200m,
            currency    = "INR",
            category    = 2,   // Accommodation
            expenseDate = DateTime.UtcNow.AddDays(-2)
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content
            .ReadFromJsonAsync<CreateExpenseResponse>();
        var expenseId = created!.ExpenseId;

        // Step 2 — Submit
        var submitResponse = await Client
            .PostAsync($"/api/expenses/{expenseId}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3 — Approve (as manager)
        AuthenticateAs(managerId, role: "Manager");
        var approveResponse = await Client
            .PostAsync($"/api/expenses/{expenseId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4 — Verify final state
        var getResponse = await Client.GetAsync($"/api/expenses/{expenseId}");
        var expense = await getResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        expense!.Status.Should().Be("2");  // Approved = 2
    }

    [Fact]
    public async Task FullRejectionFlow_DraftToRejected_Succeeds()
    {
        // Arrange
        var employeeId = NewUserId();
        var managerId  = NewUserId();

        AuthenticateAs(employeeId);
        var created = await CreateDraftExpense();

        // Submit
        await Client.PostAsync($"/api/expenses/{created.ExpenseId}/submit", null);

        // Reject (as manager)
        AuthenticateAs(managerId, role: "Manager");
        var rejectResponse = await Client.PostAsJsonAsync(
            $"/api/expenses/{created.ExpenseId}/reject",
            new { reason = "Receipt missing — please resubmit with documentation." });

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ApproveDraftExpense_Returns422()
    {
        // Cannot approve a Draft — must be Submitted first
        var employeeId = NewUserId();
        var managerId  = NewUserId();

        AuthenticateAs(employeeId);
        var created = await CreateDraftExpense();

        // Skip Submit, go straight to Approve — domain rejects this
        AuthenticateAs(managerId, role: "Manager");
        var response = await Client
            .PostAsync($"/api/expenses/{created.ExpenseId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SubmitOtherUsersExpense_Returns403()
    {
        // Employee A creates, Employee B tries to submit
        var employeeA = NewUserId();
        var employeeB = NewUserId();

        AuthenticateAs(employeeA);
        var created = await CreateDraftExpense();

        AuthenticateAs(employeeB);
        var response = await Client
            .PostAsync($"/api/expenses/{created.ExpenseId}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectWithoutReason_Returns400()
    {
        var employeeId = NewUserId();
        var managerId  = NewUserId();

        AuthenticateAs(employeeId);
        var created = await CreateDraftExpense();
        await Client.PostAsync($"/api/expenses/{created.ExpenseId}/submit", null);

        AuthenticateAs(managerId, role: "Manager");
        var response = await Client.PostAsJsonAsync(
            $"/api/expenses/{created.ExpenseId}/reject",
            new { reason = "" });   // Empty reason fails validator

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<CreateExpenseResponse> CreateDraftExpense()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses", new
        {
            description = "Test expense",
            amount      = 1000m,
            currency    = "INR",
            category    = 4,   // Office
            expenseDate = DateTime.UtcNow.AddDays(-1)
        });
        return (await response.Content.ReadFromJsonAsync<CreateExpenseResponse>())!;
    }

    private sealed record CreateExpenseResponse(
        Guid ExpenseId, string Status, DateTime CreatedAt);

    private sealed record ExpenseDto(
        Guid ExpenseId, string Status, string Description);
}
