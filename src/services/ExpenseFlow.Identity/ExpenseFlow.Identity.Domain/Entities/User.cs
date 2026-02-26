using ExpenseFlow.Shared.Domain;
using ExpenseFlow.Identity.Domain.Enums;
using ExpenseFlow.Identity.Domain.Events;
using ExpenseFlow.Identity.Domain.ValueObjects;
using ExpenseFlow.Shared.Domain.Exceptions;

namespace ExpenseFlow.Identity.Domain.Entities;

/// <summary>
/// User Aggregate Root — the central entity of the Identity bounded context.
/// All state changes happen through explicit domain methods (no public setters).
/// Raises domain events that are dispatched after the transaction commits.
/// Source: Microsoft Microservices Guide, Ch. 7
/// </summary>
public sealed class User : AggregateRoot
{
    public Email Email { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private User() { } // Required by EF Core

    private User(Guid id, Email email, string firstName, string lastName,
        string passwordHash, UserRole role) : base(id)
    {
        Email = email;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRegisteredEvent(Id, Email.Value, FullName, Role));
    }

    public static User Create(Email email, string firstName, string lastName,
        string passwordHash, UserRole role = UserRole.Employee)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        return new User(Guid.NewGuid(), email, firstName, lastName, passwordHash, role);
    }

    public void RecordLogin()
    {
        if (Status != UserStatus.Active)
            throw new DomainException($"Cannot login: account is {Status}.");
        LastLoginAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public bool HasValidRefreshToken(string token)
        => RefreshToken == token && RefreshTokenExpiresAt > DateTime.UtcNow;

    public void Deactivate(string reason)
    {
        if (Status == UserStatus.Deactivated)
            throw new DomainException("User is already deactivated.");
        Status = UserStatus.Deactivated;
        RevokeRefreshToken();
        AddDomainEvent(new UserDeactivatedEvent(Id, Email.Value, reason));
    }

    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        RevokeRefreshToken(); // Force re-login after password change
    }
}
