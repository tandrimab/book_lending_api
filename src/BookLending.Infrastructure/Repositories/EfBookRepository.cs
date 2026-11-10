using BookLending.Domain.Items;
using BookLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BookLending.Infrastructure.Repositories;

public class EfBookRepository : IBookRepository
{
    private readonly BookLendingDbContext _context;
    public EfBookRepository(BookLendingDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Book> GetAllBooks()
    {
        return _context.Books.AsNoTracking().ToList();
    }

    public Book? GetBookBYIsbn(string isbn)
    {
        return _context.Books.AsNoTracking().FirstOrDefault(b => b.Isbn == new IsbnNumber(isbn));
    }

    public void AddBook(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();
    }


}