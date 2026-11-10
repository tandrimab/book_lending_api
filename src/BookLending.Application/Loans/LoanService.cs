using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Domain.Loans;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
namespace BookLending.Application.Loans;

public class LoanService
{
    private const string StatsCacheKey = "loans:stats";

    // Overdue count depends on the clock, so even with no writes the cached value
    // drifts. A short expiry bounds how stale it can get.
    private static readonly DistributedCacheEntryOptions StatsCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private readonly ILoanRepository _loanRepository;
    private readonly IDistributedCache _cache;

    public LoanService(ILoanRepository loanRepository, IDistributedCache cache)
    {
        _loanRepository = loanRepository;
        _cache = cache;
    }

    public IEnumerable<Loan> GetAllLoans() => _loanRepository.GetAllLoans();

    public Loan? GetLoanById(string id) => _loanRepository.GetLoanById(id);

    public IEnumerable<(Loan Loan, string BookTitle)> GetLoansWithBookTitles() =>
        _loanRepository.GetLoansWithBookTitles();

    public async Task<Loan> LendBookAsync(Member member, Book book)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (book == null) throw new ArgumentNullException(nameof(book));

        var loanDate = DateTime.UtcNow;

        var loan = new Loan(member.Id, book.Id, loanDate, null, book.LoanPeriod());
        _loanRepository.AddLoan(loan);
        await InvalidateStatsAsync(); // total just changed
        return loan;
    }

    public async Task ReturnBookAsync(Loan loan)
    {
        if (loan == null) throw new ArgumentNullException(nameof(loan));
        if (!loan.IsActive()) throw new InvalidOperationException("Loan is not active.");

        loan.MarkAsReturned(DateTime.UtcNow);
        _loanRepository.Update(loan);
        await InvalidateStatsAsync();
    }

    public IEnumerable<Loan> GetOverdueLoans(DateTime currentDate) =>
        _loanRepository.GetOverdueLoans(currentDate);

    public record LoanStats(int Total, int Overdue);

    public async Task<LoanStats> GetLoanStatsAsync()
    {
        var cached = await _cache.GetStringAsync(StatsCacheKey);
        if (cached is not null)
            return JsonSerializer.Deserialize<LoanStats>(cached)!;

        var stats = new LoanStats(
            _loanRepository.GetAllLoans().Count(),
            _loanRepository.GetOverdueLoans(DateTime.UtcNow).Count());

        await _cache.SetStringAsync(StatsCacheKey, JsonSerializer.Serialize(stats), StatsCacheOptions);
        return stats;
    }

    private Task InvalidateStatsAsync() => _cache.RemoveAsync(StatsCacheKey);
}
