namespace ExpenseFlow.Identity.Application.DTOs;

public sealed record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
