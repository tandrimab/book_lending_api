using BookLending.Domain.Items;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Api.Controllers;

public record CreateBookRequest(string Title, string Author, int Year, string Publisher, string Isbn);

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _books;

    public BooksController(IBookRepository books) => _books = books;
    [HttpGet]
    public IActionResult GetAll() => Ok(_books.GetAllBooks());

    [HttpGet("{isbn}")]
    public IActionResult GetByIsbn(string isbn)
    {
        var book = _books.GetBookBYIsbn(isbn);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public IActionResult Create(CreateBookRequest request)
    {
        if (_books.GetBookBYIsbn(request.Isbn) is not null)
            return Conflict($"Book with ISBN {request.Isbn} already exists.");   // 409

        var book = new Book(request.Title, request.Author, request.Year, request.Publisher, new IsbnNumber(request.Isbn));
        _books.AddBook(book);

        return CreatedAtAction(nameof(GetByIsbn), new { isbn = request.Isbn }, book);
    }
}
