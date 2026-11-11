namespace BookLending.Domain.Loans
{
    public class Loan
    {
        private Loan() { }
        public string Id { get; private set; } = null!;

        public string MemberId { get; set; } = null!;
        public string BookId { get; private set; } = null!;

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedOn { get; set; }

        public Loan(string memberId, string bookId, DateTime loanDate, DateTime? returnedOn, int loanDays)
        {
            Id = Guid.NewGuid().ToString();
            MemberId = memberId;
            BookId = bookId;
            LoanDate = loanDate;
            DueDate = loanDate.AddDays(loanDays);
            ReturnedOn = returnedOn;
        }

        public static Loan Restore(string id, string memberId, string bookId,
            DateTime loanDate, DateTime dueDate, DateTime? returnedOn) => new()
            {
                Id = id,
                MemberId = memberId,
                BookId = bookId,
                LoanDate = loanDate,
                DueDate = dueDate,
                ReturnedOn = returnedOn
            };

        public bool IsActive() => ReturnedOn == null;

        public bool IsOverdue() => IsActive() && DateTime.UtcNow > DueDate;

        public int DaysOverDue() => IsOverdue() ? (DateTime.UtcNow - DueDate).Days : 0;

        public void MarkAsReturned(DateTime returnedOn)
        {
            ReturnedOn = returnedOn;
        }
    }
}
