using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookLending.Domain.Loans;

namespace BookLending.Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasMaxLength(36).IsRequired();
        builder.Property(l => l.BookId).HasMaxLength(36).IsRequired();
        builder.Property(l => l.MemberId).HasMaxLength(36).IsRequired();
    }
}