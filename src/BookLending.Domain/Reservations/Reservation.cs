namespace BookLending.Domain.Reservations;

public enum ReservationStatus
{
    Waiting,
    Notified,
    Fulfilled,
    Expired,
    Cancelled
}

public class Reservation
{
    private Reservation() { }

    public string Id { get; private set; } = null!;
    public string BookId { get; private set; } = null!;
    public string MemberId { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime? HoldExpiresAt { get; private set; }
    public string? TriggeredByLoanId { get; private set; }

    public Reservation(string bookId, string memberId, DateTime createdAt)
    {
        Id = Guid.NewGuid().ToString();
        BookId = bookId;
        MemberId = memberId;
        CreatedAt = createdAt;
        Status = ReservationStatus.Waiting;
    }

    public bool IsActive() => Status is ReservationStatus.Waiting or ReservationStatus.Notified;

    public void MarkNotified(string loanId, DateTime now, TimeSpan holdFor)
    {
        if (Status != ReservationStatus.Waiting)
            throw new InvalidOperationException($"Reservation '{Id}' is {Status}, not Waiting.");

        Status = ReservationStatus.Notified;
        NotifiedAt = now;
        HoldExpiresAt = now.Add(holdFor);
        TriggeredByLoanId = loanId;
    }
}
