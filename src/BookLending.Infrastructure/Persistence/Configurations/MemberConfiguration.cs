using Microsoft.EntityFrameworkCore;
using BookLending.Domain.Members;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLending.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasMaxLength(36).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.ComplexProperty(b => b.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(100).IsRequired();
            address.Property(a => a.ZipCode).HasMaxLength(20).IsRequired();
        });
    }
}