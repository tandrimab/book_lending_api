using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using Microsoft.AspNetCore.Mvc;
using BookLending.Application.Loans;
using Microsoft.AspNetCore.Authorization;
namespace BookLending.Api.Controllers;

public record LendBookRequest(string MemberId, string Isbn);

public record LoanResponse(
    string Id,
    string MemberId,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnedOn,
    bool IsActive,
    bool IsOverdue,
    int DaysOverdue,
    string? BookTitle)
{
    // Endpoints that don't load the book: BookTitle is null ("not loaded"), never a fake "".
    public static LoanResponse From(Loan loan) => From(loan, null);

    public static LoanResponse From(Loan loan, string? bookTitle) => new(
        loan.Id, loan.MemberId, loan.LoanDate, loan.DueDate, loan.ReturnedOn,
        loan.IsActive(), loan.IsOverdue(), loan.DaysOverDue(), bookTitle);
}

[ApiController]
[Authorize(Roles = "Librarian")]
[Route("loans")]
public class LoansController : ControllerBase
{
    private readonly LoanService _loanService;
    private readonly IMemberRepository _members;
    private readonly IBookRepository _books;

    public LoansController(LoanService loanService, IMemberRepository members, IBookRepository books)
    {
        _loanService = loanService;
        _members = members;
        _books = books;
    }

    [HttpGet]
    public ActionResult<IEnumerable<LoanResponse>> GetAll() =>
        Ok(_loanService.GetLoansWithBookTitles().Select(x => LoanResponse.From(x.Loan, x.BookTitle)));

    [HttpGet("{id}")]
    public ActionResult<LoanResponse> GetById(string id)
    {
        var loan = _loanService.GetLoanById(id);
        if (loan is null) return NotFound();
        return LoanResponse.From(loan);
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var (total, overdue) = _loanService.GetLoanStats();
        return Ok(new { total, overdue });
    }

    [HttpGet("overdue")]
    public ActionResult<IEnumerable<LoanResponse>> GetOverdue() =>
        Ok(_loanService.GetOverdueLoans(DateTime.UtcNow).Select(LoanResponse.From));

    [HttpPost]
    public ActionResult<LoanResponse> LendBook(LendBookRequest request)
    {

        var member = _members.GetMemberById(request.MemberId);
        if (member is null) return NotFound($"Member '{request.MemberId}' not found.");

        var book = _books.GetBookBYIsbn(request.Isbn);
        if (book is null) return NotFound($"Book with ISBN '{request.Isbn}' not found.");

        var loan = _loanService.LendBook(member, book);

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, LoanResponse.From(loan));
    }

    [HttpPost("{id}/return")]
    public ActionResult<LoanResponse> ReturnBook(string id)
    {
        var loan = _loanService.GetLoanById(id);
        if (loan is null) return NotFound();

        if (!loan.IsActive()) return Conflict($"Loan '{id}' has already been returned.");

        _loanService.ReturnBook(loan);
        return LoanResponse.From(loan);
    }
}
