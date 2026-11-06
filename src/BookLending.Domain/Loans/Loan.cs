namespace BookLending.Domain.Loans
{
    public class Loan
    {
        public static int _idCounter = 0;
        public string Id { get; private set; }

        public string MemberId { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedOn { get; set; }

        public Loan(string memberId, DateTime loanDate, DateTime? returnedOn, int loanDays)
        {
            Id = (++_idCounter).ToString();
            MemberId = memberId;
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