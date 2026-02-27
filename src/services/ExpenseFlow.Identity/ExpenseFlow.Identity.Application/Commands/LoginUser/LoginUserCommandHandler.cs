using MediatR;
using ExpenseFlow.Identity.Application.DTOs;
using ExpenseFlow.Identity.Application.Interfaces;
using ExpenseFlow.Identity.Domain.Exceptions;
using ExpenseFlow.Identity.Domain.Interfaces;

namespace ExpenseFlow.Identity.Application.Commands.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public LoginUserCommandHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct)
            ?? throw new UserNotFoundException(cmd.Email);

        if (!_hasher.Verify(cmd.Password, user.PasswordHash))
            throw new InvalidPasswordException();

        user.RecordLogin();

        var (access, refresh, expiresAt) = _jwt.GenerateTokens(user);
        user.SetRefreshToken(refresh, expiresAt);

        _repo.Update(user);
        await _repo.UnitOfWork.SaveEntitiesAsync(ct);

        return new AuthResponseDto(
            access, refresh,
            user.Id, user.FullName, user.Email.Value, user.Role.ToString());
    }
}
