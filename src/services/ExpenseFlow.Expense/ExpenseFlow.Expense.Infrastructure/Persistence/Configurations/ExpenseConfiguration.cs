using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ExpenseFlow.Expense.Domain.Entities;
using ExpenseFlow.Expense.Domain.Enums;
using ExpenseFlow.Expense.Domain.ValueObjects;

namespace ExpenseFlow.Expense.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the Expense aggregate to the Expenses table.
/// Keeps all persistence concerns (column names, lengths, indexes)
/// entirely out of the domain model.
///
/// Key mappings:
///   Money      → owned type  → Amount_Value + Amount_Currency columns
///   Description→ owned type  → Description_Value column
///   Status     → stored as int  (0=Draft, 1=Submitted, 2=Approved, 3=Rejected)
///   Category   → stored as int
/// </summary>
public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.SubmittedByUserId)
            .IsRequired();

        // Money value object — inline columns, not a separate table
        builder.OwnsOne(e => e.Amount, money =>
        {
            money.Property(m => m.Amount)
                 .HasColumnName("Amount_Value")
                 .HasColumnType("decimal(18,2)")
                 .IsRequired();

            money.Property(m => m.Currency)
                 .HasColumnName("Amount_Currency")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        // ExpenseDescription value object — inline
        builder.OwnsOne(e => e.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(ExpenseDescription.MaxLength)
                .IsRequired();
        });

        builder.Property(e => e.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ExpenseDate)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.SubmittedAt);
        builder.Property(e => e.ReviewedAt);
        builder.Property(e => e.ReviewedByUserId);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(1000);

        // Query indexes — most common read patterns
        builder.HasIndex(e => e.SubmittedByUserId)
            .HasDatabaseName("IX_Expenses_SubmittedByUserId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Expenses_Status");

        // Domain events are transient — never persisted
        builder.Ignore(e => e.DomainEvents);
    }
}
