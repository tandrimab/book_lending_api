using BookLending.Domain.Items;
using BookLending.Domain.Members;

namespace BookLending.Domain.Loans;

public class LoanService
{
    private readonly ILoanRepository _loanRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public IEnumerable<Loan> GetAllLoans() => _loanRepository.GetAllLoans();

    public Loan? GetLoanById(string id) => _loanRepository.GetLoanById(id);

    public IEnumerable<(Loan Loan, string BookTitle)> GetLoansWithBookTitles() =>
        _loanRepository.GetLoansWithBookTitles();

    public Loan LendBook(Member member, Book book)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (book == null) throw new ArgumentNullException(nameof(book));

        var loanDate = DateTime.UtcNow;

        var loan = new Loan(member.Id, book.Id, loanDate, null, book.LoanPeriod());
        _loanRepository.AddLoan(loan);
        return loan;
    }

    public void ReturnBook(Loan loan)
    {
        if (loan == null) throw new ArgumentNullException(nameof(loan));
        if (!loan.IsActive()) throw new InvalidOperationException("Loan is not active.");

        loan.MarkAsReturned(DateTime.UtcNow);
        _loanRepository.Update(loan);
    }

    public IEnumerable<Loan> GetOverdueLoans(DateTime currentDate) =>
        _loanRepository.GetOverdueLoans(currentDate);

    public (int total, int overDue) GetLoanStats()
    {
        int totalLoans = _loanRepository.GetAllLoans().Count();
        int overDueItems = _loanRepository.GetOverdueLoans(DateTime.UtcNow).Count();
        return (totalLoans, overDueItems);
    }
}
