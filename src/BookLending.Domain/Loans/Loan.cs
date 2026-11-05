namespace BookLending.Domain.Loans
{
    public class Loan
    {
        public string Id { get; private set; }
        public int Tenure { get; set; }
        public int Total { get; set; }
        public int Overdue { get; set; }

        public Loan(string title, string author, int year)
        {
            Id = Guid.NewGuid().ToString();

        }

        public (int total, int overdue) CalculateTotalAndOverdue(int tenure, int year)
        {
            Total = tenure * 10;
            Overdue = (DateTime.Now.Year - year) * 5;
            return (Total, Overdue);
        }
    }
}