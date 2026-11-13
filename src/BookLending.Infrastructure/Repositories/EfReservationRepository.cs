using BookLending.Domain.Reservations;
using BookLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLending.Infrastructure.Repositories;

public class EfReservationRepository : IReservationRepository
{
    private readonly BookLendingDbContext _context;

    public EfReservationRepository(BookLendingDbContext context)
    {
        _context = context;
    }

    public void Add(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        _context.SaveChanges();
    }

    public void Update(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        _context.SaveChanges();
    }

    public Reservation? GetNextWaiting(string bookId)
    {
        return _context.Reservations
            .AsNoTracking()
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Waiting)
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefault();
    }

    public bool HasActiveReservation(string bookId, string memberId)
    {
        return _context.Reservations.Any(r =>
            r.BookId == bookId &&
            r.MemberId == memberId &&
            (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Notified));
    }

    public bool ExistsForLoan(string loanId)
    {
        return _context.Reservations.Any(r => r.TriggeredByLoanId == loanId);
    }
}
