using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseFlow.Expense.Application.Commands.ApproveExpense;
using ExpenseFlow.Expense.Application.Commands.CreateExpense;
using ExpenseFlow.Expense.Application.Commands.RejectExpense;
using ExpenseFlow.Expense.Application.Commands.SubmitExpense;
using ExpenseFlow.Expense.Application.DTOs;
using ExpenseFlow.Expense.Application.Queries.GetExpenseById;
using ExpenseFlow.Expense.Application.Queries.GetExpensesByStatus;
using ExpenseFlow.Expense.Application.Queries.GetExpensesByUser;
using ExpenseFlow.Expense.Domain.Enums;

namespace ExpenseFlow.Expense.API.Controllers;

/// <summary>
/// REST endpoints for the full expense lifecycle.
///
/// Route design:
///   POST   /api/expenses              → Create (Employee)
///   POST   /api/expenses/{id}/submit  → Submit  (owner only)
///   POST   /api/expenses/{id}/approve → Approve (Manager/Admin)
///   POST   /api/expenses/{id}/reject  → Reject  (Manager/Admin)
///   GET    /api/expenses/{id}         → Get by ID (authenticated)
///   GET    /api/expenses/my           → My expenses (authenticated)
///   GET    /api/expenses/pending      → Approval queue (Manager/Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator) => _mediator = mediator;

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// Extracts the authenticated user's ID from the JWT sub claim.
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim is missing."));

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>Creates a new expense in Draft status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateExpenseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExpenseRequest request,
        CancellationToken ct)
    {
        var command = new CreateExpenseCommand(
            SubmittedByUserId: CurrentUserId,
            Description:       request.Description,
            Amount:            request.Amount,
            Currency:          request.Currency,
            Category:          request.Category,
            ExpenseDate:       request.ExpenseDate);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.ExpenseId }, result);
    }

    /// <summary>Submits a Draft expense for approval. Only the owner can submit.</summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit([FromRoute] Guid id, CancellationToken ct)
    {
        await _mediator.Send(new SubmitExpenseCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    /// <summary>Approves a Submitted expense. Requires Manager or Admin role.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ApproveExpenseCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    /// <summary>Rejects a Submitted expense with a mandatory reason. Requires Manager or Admin role.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id,
        [FromBody]  RejectExpenseRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(
            new RejectExpenseCommand(id, CurrentUserId, request.Reason), ct);
        return NoContent();
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Returns a single expense by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExpenseByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Returns all expenses submitted by the currently authenticated user.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetExpensesByUserQuery(CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns all expenses with Submitted status (the approval queue).
    /// Requires Manager or Admin role.
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetExpensesByStatusQuery(ExpenseStatus.Submitted), ct);
        return Ok(result);
    }
}

// ── Request models (thin wrappers — keep Command records clean of [FromBody]) ───

public sealed record CreateExpenseRequest(
    string          Description,
    decimal         Amount,
    string          Currency,
    ExpenseCategory Category,
    DateTime        ExpenseDate);

public sealed record RejectExpenseRequest(string Reason);
