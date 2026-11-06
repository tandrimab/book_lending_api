using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;

public class LoanService
{
    public List<Loan> LoanItems { get; private set; } = new List<Loan>();
    public Loan LendBook(Member member, Book book)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (book == null) throw new ArgumentNullException(nameof(book));

        var loanDate = DateTime.Now;

        var loan = new Loan(member.Id, loanDate, null, book.LoanPeriod());
        LoanItems.Add(loan);
        return loan;
    }

    public void ReturnBook(Loan loan)
    {
        if (loan == null) throw new ArgumentNullException(nameof(loan));
        if (!loan.IsActive()) throw new InvalidOperationException("Loan is not active.");

        loan.MarkAsReturned(DateTime.Now);
    }

    public IEnumerable<Loan> GetOverdueLoans(DateTime currentDate)
    {
        IEnumerable<Loan> overDueLoans =
        from loan in LoanItems
        where loan.IsOverdue()
        select loan;
        return overDueLoans;
    }

    public (int total, int overDue) GetLoanStats()
    {
        int totalLoans = LoanItems.Count;
        int overDueItems = LoanItems.Count(loan => loan.IsOverdue());
        return (totalLoans, overDueItems);
    }
}
