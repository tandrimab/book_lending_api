using BookLending.Application.Loans.Events;
using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Domain.Reminders;
using BookLending.Domain.Reservations;

namespace BookLending.Application.Reservations;

public enum BookReturnedOutcome
{
    MemberNotified,
    NoWaitlist,
    AlreadyHandled
}

public class ReservationService
{
    private static readonly TimeSpan HoldPeriod = TimeSpan.FromHours(48);

    private readonly IReservationRepository _reservations;
    private readonly IMemberRepository _members;
    private readonly IReminderSender _reminderSender;

    public ReservationService(
        IReservationRepository reservations,
        IMemberRepository members,
        IReminderSender reminderSender)
    {
        _reservations = reservations;
        _members = members;
        _reminderSender = reminderSender;
    }

    public Reservation ReserveBook(Member member, Book book)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (book == null) throw new ArgumentNullException(nameof(book));

        if (_reservations.HasActiveReservation(book.Id, member.Id))
            throw new InvalidOperationException("Member already has an active reservation for this book.");

        var reservation = new Reservation(book.Id, member.Id, DateTime.UtcNow);
        _reservations.Add(reservation);
        return reservation;
    }

    public async Task<BookReturnedOutcome> HandleBookReturnedAsync(BookReturnedEvent evt)
    {
        if (_reservations.ExistsForLoan(evt.LoanId))
            return BookReturnedOutcome.AlreadyHandled;

        var next = _reservations.GetNextWaiting(evt.BookId);
        if (next is null)
            return BookReturnedOutcome.NoWaitlist;

        var member = _members.GetMemberById(next.MemberId)
            ?? throw new InvalidOperationException($"Member '{next.MemberId}' not found.");

        next.MarkNotified(evt.LoanId, DateTime.UtcNow, HoldPeriod);

        await _reminderSender.SendReminderAsync(member,
            $"Good news! A book you reserved is available. We'll hold it for you until {next.HoldExpiresAt:dd MMM yyyy HH:mm} UTC.");

        _reservations.Update(next);
        return BookReturnedOutcome.MemberNotified;
    }
}
