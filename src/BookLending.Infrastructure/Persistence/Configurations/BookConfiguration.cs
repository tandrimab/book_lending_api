using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookLending.Domain.Items;

namespace BookLending.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasMaxLength(36).IsRequired();
        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Author).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Publisher).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Isbn).HasConversion(isbn => isbn.Value, value => new IsbnNumber(value)).HasMaxLength(20).IsRequired();
        builder.HasIndex(b => b.Isbn).IsUnique();
    }
}