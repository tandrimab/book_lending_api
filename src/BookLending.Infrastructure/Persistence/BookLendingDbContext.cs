using Microsoft.EntityFrameworkCore;
using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using BookLending.Domain.Reservations;
namespace BookLending.Infrastructure.Persistence;

public class BookLendingDbContext : DbContext
{
    public BookLendingDbContext(DbContextOptions<BookLendingDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookLendingDbContext).Assembly);
    }
}