using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using BookLending.Domain.Items;

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
        builder.HasOne<Member>().WithMany().HasForeignKey(l => l.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Book>().WithMany().HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}