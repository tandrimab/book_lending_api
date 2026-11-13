using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookLending.Domain.Reservations;
using BookLending.Domain.Members;
using BookLending.Domain.Items;

namespace BookLending.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasMaxLength(36).IsRequired();
        builder.Property(r => r.BookId).HasMaxLength(36).IsRequired();
        builder.Property(r => r.MemberId).HasMaxLength(36).IsRequired();
        builder.Property(r => r.TriggeredByLoanId).HasMaxLength(36);

        // Store the enum as readable text ("Waiting") instead of a number.
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne<Member>().WithMany().HasForeignKey(r => r.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Book>().WithMany().HasForeignKey(r => r.BookId).OnDelete(DeleteBehavior.Restrict);

        // Fast "next in line" lookup.
        builder.HasIndex(r => new { r.BookId, r.Status, r.CreatedAt });

        // Database-level idempotency guard: one loan's return can trigger at most one hold.
        builder.HasIndex(r => r.TriggeredByLoanId)
            .IsUnique()
            .HasFilter("[TriggeredByLoanId] IS NOT NULL");
    }
}
