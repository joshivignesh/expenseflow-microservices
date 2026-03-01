using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Identity.Domain.Enums;

namespace ExpenseFlow.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the User aggregate to the database schema.
/// Keeps all persistence concerns (column names, lengths, indexes) out of the domain model.
/// The Email value object is mapped as an owned type so its Value column is inlined into Users.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever(); // We generate GUIDs in the domain

        // Owned value object — maps Email.Value → Users.Email_Value
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("Email_Value")
                 .HasMaxLength(256)
                 .IsRequired();

            email.HasIndex(e => e.Value)
                 .IsUnique()
                 .HasDatabaseName("IX_Users_Email_Unique");
        });

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(512);

        builder.Property(u => u.RefreshTokenExpiresAt);

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        // Ignore domain events — they are transient and never persisted
        builder.Ignore(u => u.DomainEvents);
    }
}
