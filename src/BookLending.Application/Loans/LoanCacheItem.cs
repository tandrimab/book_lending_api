using BookLending.Domain.Loans;
namespace BookLending.Application.Loans;

internal record LoanCacheItem(
    string Id,
    string MemberId,
    string BookId,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnedOn)
{
    public static LoanCacheItem From(Loan loan) => new(
        loan.Id, loan.MemberId, loan.BookId, loan.LoanDate, loan.DueDate, loan.ReturnedOn);

    public Loan ToLoan() => Loan.Restore(Id, MemberId, BookId, LoanDate, DueDate, ReturnedOn);
}
