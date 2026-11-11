using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Domain.Loans;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using BookLending.Application.Messaging;
namespace BookLending.Application.Loans;

public class LoanService
{
    private const string StatsCacheKey = "loans:stats";
    private static string LoanCacheKey(string id) => $"loan:{id}";

    private static readonly DistributedCacheEntryOptions StatsCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private static readonly DistributedCacheEntryOptions LoanCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private readonly ILoanRepository _loanRepository;
    private readonly IDistributedCache _cache;
    private readonly IMessagePublisher _publisher;

    public LoanService(ILoanRepository loanRepository, IDistributedCache cache, IMessagePublisher publisher)
    {
        _loanRepository = loanRepository;
        _cache = cache;
        _publisher = publisher;
    }

    public IEnumerable<Loan> GetAllLoans() => _loanRepository.GetAllLoans();

    public Loan? GetLoanById(string id) => _loanRepository.GetLoanById(id);

    // Cached, for READS. Cache-aside, same shape as GetLoanStatsAsync.
    public async Task<Loan?> GetLoanByIdAsync(string id)
    {
        var key = LoanCacheKey(id);

        var cached = await _cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<LoanCacheItem>(cached)!.ToLoan();

        var loan = _loanRepository.GetLoanById(id);
        if (loan is null)
            return null;

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(LoanCacheItem.From(loan)), LoanCacheOptions);
        return loan;
    }

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

        // Hand the reminder off to the background instead of sending it here.
        // The request returns as soon as the message is on the queue.
        await _publisher.PublishAsync(new LoanCreated(loan.Id, loan.MemberId, loan.BookId, loan.DueDate));
        return loan;
    }

    public async Task ReturnBookAsync(Loan loan)
    {
        if (loan == null) throw new ArgumentNullException(nameof(loan));
        if (!loan.IsActive()) throw new InvalidOperationException("Loan is not active.");

        loan.MarkAsReturned(DateTime.UtcNow);
        _loanRepository.Update(loan);
        await _cache.RemoveAsync(LoanCacheKey(loan.Id));
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
