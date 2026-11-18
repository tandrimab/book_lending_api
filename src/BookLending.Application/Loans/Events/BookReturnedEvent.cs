namespace BookLending.Application.Loans.Events;

public record BookReturnedEvent(string LoanId, string BookId, DateTime ReturnedOn);
