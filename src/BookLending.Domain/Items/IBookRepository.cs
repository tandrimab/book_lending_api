namespace BookLending.Domain.Items
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAllBooks();
        Book? GetBookBYIsbn(string isbn);
        void AddBook(Book book);
    }
}