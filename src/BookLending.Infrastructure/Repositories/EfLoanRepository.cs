using BookLending.Infrastructure.Persistence;
using BookLending.Domain.Loans;
using Microsoft.EntityFrameworkCore;

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
            return _context.Loans.AsNoTracking().ToList();
        }

        public Loan? GetLoanById(string id)
        {
            return _context.Loans.AsNoTracking().FirstOrDefault(l => l.Id == id);
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
                .AsNoTracking()
                .ToList();
        }

        public void Update(Loan loan)
        {
            _context.Loans.Update(loan);
            _context.SaveChanges();
        }

        public IEnumerable<(Loan Loan, string BookTitle)> GetLoansWithBookTitles()
        {
            // TODO: join _context.Loans with _context.Books on l.BookId == b.Id
            //       select (l, b.Title), AsNoTracking, ToList
            //       Query syntax may read easier here:
            //       from l in _context.Loans
            //       join b in _context.Books on l.BookId equals b.Id
            //       select new { l, b.Title }
            var query = from l in _context.Loans
                        join b in _context.Books on l.BookId equals b.Id
                        select new { Loan = l, BookTitle = b.Title };
            return query.AsNoTracking().ToList().Select(x => (x.Loan, x.BookTitle));
        }


    }
}