using BookLending.Infrastructure.Persistence;
using BookLending.Domain.Loans;

namespace BookLending.Infrastructure.Repositories
{
    public class EfLoanRepository : ILoanRepository
    {
        private readonly BookLendingDbContext _context;

        public EfLoanRepository(BookLendingDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Loan> GetAllLoans()
        {
            return _context.Loans.ToList();
        }

        public Loan? GetLoanById(string id)
        {
            return _context.Loans.FirstOrDefault(l => l.Id == id);
        }

        public void AddLoan(Loan loan)
        {
            _context.Loans.Add(loan);
            _context.SaveChanges();
        }

        public IEnumerable<Loan> GetOverdueLoans(DateTime asOf)
        {
            return _context.Loans
                .Where(l => l.DueDate < asOf && l.ReturnedOn == null)
                .ToList();
        }

        public void Update(Loan loan)
        {
            _context.Loans.Update(loan);
            _context.SaveChanges();
        }

    }
}