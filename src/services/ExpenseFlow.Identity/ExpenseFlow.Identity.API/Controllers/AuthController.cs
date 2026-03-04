using Microsoft.AspNetCore.Mvc;
using MediatR;
using ExpenseFlow.Identity.Application.Commands.RegisterUser;
using ExpenseFlow.Identity.Application.Commands.LoginUser;
using ExpenseFlow.Identity.Application.Queries.GetUserProfile;
using ExpenseFlow.Identity.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseFlow.Identity.API.Controllers;

/// <summary>
/// Handles all authentication and identity HTTP operations.
/// Intentionally thin — no business logic lives here.
/// Every request is translated into a MediatR command or query and dispatched.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Registers a new user account.
    /// Returns 201 Created with JWT tokens on success.
    /// Returns 400 if validation fails, 409 if email is already taken.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetProfile), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Authenticates an existing user.
    /// Returns 200 OK with JWT tokens on success.
    /// Returns 401 if credentials are invalid.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the profile of the specified user.
    /// Requires a valid Bearer JWT token.
    /// Returns 404 if the user does not exist.
    /// </summary>
    [HttpGet("profile/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(
        [FromRoute] Guid userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(userId), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
