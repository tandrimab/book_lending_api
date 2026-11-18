namespace BookLending.Domain.Reservations;

public interface IReservationRepository
{
    void Add(Reservation reservation);
    void Update(Reservation reservation);

    Reservation? GetNextWaiting(string bookId);

    bool HasActiveReservation(string bookId, string memberId);

    bool ExistsForLoan(string loanId);
}
