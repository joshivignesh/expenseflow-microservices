using ExpenseFlow.Identity.Domain.Entities;

namespace ExpenseFlow.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user);
}
