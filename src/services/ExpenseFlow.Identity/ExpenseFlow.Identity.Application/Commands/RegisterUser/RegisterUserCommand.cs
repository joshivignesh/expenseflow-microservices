using MediatR;
using ExpenseFlow.Identity.Application.DTOs;
using ExpenseFlow.Identity.Domain.Enums;

namespace ExpenseFlow.Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role = UserRole.Employee
) : IRequest<AuthResponseDto>;
