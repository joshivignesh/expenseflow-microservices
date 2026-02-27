using MediatR;
using ExpenseFlow.Identity.Application.DTOs;
using ExpenseFlow.Identity.Application.Interfaces;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Identity.Domain.Exceptions;
using ExpenseFlow.Identity.Domain.Interfaces;
using ExpenseFlow.Identity.Domain.ValueObjects;

namespace ExpenseFlow.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public RegisterUserCommandHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = Email.Create(cmd.Email);

        if (await _repo.ExistsWithEmailAsync(email.Value, ct))
            throw new DuplicateEmailException(email.Value);

        var user = User.Create(
            email,
            cmd.FirstName,
            cmd.LastName,
            _hasher.Hash(cmd.Password),
            cmd.Role);

        var (access, refresh, expiresAt) = _jwt.GenerateTokens(user);
        user.SetRefreshToken(refresh, expiresAt);

        await _repo.AddAsync(user, ct);
        await _repo.UnitOfWork.SaveEntitiesAsync(ct);

        return new AuthResponseDto(
            access, refresh,
            user.Id, user.FullName, user.Email.Value, user.Role.ToString());
    }
}
