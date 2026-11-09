namespace BookLending.Domain.Loans
{
    public interface ILoanRepository
    {
        IEnumerable<Loan> GetAllLoans();
        Loan? GetLoanById(string id);
        IEnumerable<Loan> GetOverdueLoans(DateTime asOf);
        void AddLoan(Loan loan);
        void Update(Loan loan);
    }
}
