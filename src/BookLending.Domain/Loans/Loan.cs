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

        public bool IsActive() => ReturnedOn == null;

        public bool IsOverdue() => IsActive() && DateTime.Now > DueDate;

        public int DaysOverDue() => IsOverdue() ? (DateTime.Now - DueDate).Days : 0;

        public void MarkAsReturned(DateTime returnedOn)
        {
            ReturnedOn = returnedOn;
        }
    }
}
