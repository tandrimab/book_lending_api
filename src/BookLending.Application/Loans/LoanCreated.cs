namespace BookLending.Application.Loans;

public record LoanCreated(string LoanId, string MemberId, string BookId, DateTime DueDate);
