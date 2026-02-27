using MediatR;
using ExpenseFlow.Identity.Application.DTOs;

namespace ExpenseFlow.Identity.Application.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>;
