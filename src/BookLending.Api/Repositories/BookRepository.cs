using System.Collections.Concurrent;
using BookLending.Domain.Items;

namespace BookLending.Api.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<string, Book> _books = new();

    public IEnumerable<Book> GetAllBooks() => _books.Values;

    public Book? GetBookBYIsbn(string isbn) => _books.GetValueOrDefault(isbn);

    public void AddBook(Book book)
    {
        if (!_books.TryAdd(book.Isbn.Value, book))
            throw new InvalidOperationException($"Book with ISBN {book.Isbn.Value} already exists.");
    }
}
