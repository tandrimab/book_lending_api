using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using Microsoft.AspNetCore.Mvc;

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
    int DaysOverdue)
{
    public static LoanResponse From(Loan loan) => new(
        loan.Id, loan.MemberId, loan.LoanDate, loan.DueDate, loan.ReturnedOn,
        loan.IsActive(), loan.IsOverdue(), loan.DaysOverDue());
}

[ApiController]
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
    public IActionResult GetAll() =>
        Ok(_loanService.GetAllLoans().Select(LoanResponse.From));

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var loan = _loanService.GetLoanById(id);
        return loan is null ? NotFound() : Ok(LoanResponse.From(loan));
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var (total, overdue) = _loanService.GetLoanStats();
        return Ok(new { total, overdue });
    }

    [HttpGet("overdue")]
    public IActionResult GetOverdue() =>
        Ok(_loanService.GetOverdueLoans(DateTime.Now).Select(LoanResponse.From));

    [HttpPost]
    public IActionResult LendBook(LendBookRequest request)
    {

        var member = _members.GetMemberById(request.MemberId);
        if (member is null) return NotFound($"Member '{request.MemberId}' not found.");

        var book = _books.GetBookBYIsbn(request.Isbn);
        if (book is null) return NotFound($"Book with ISBN '{request.Isbn}' not found.");

        var loan = _loanService.LendBook(member, book);

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, LoanResponse.From(loan));
    }

    [HttpPost("{id}/return")]
    public IActionResult ReturnBook(string id)
    {
        var loan = _loanService.GetLoanById(id);
        if (loan is null) return NotFound();

        if (!loan.IsActive()) return Conflict($"Loan '{id}' has already been returned.");

        _loanService.ReturnBook(loan);
        return Ok(LoanResponse.From(loan));
    }
}
