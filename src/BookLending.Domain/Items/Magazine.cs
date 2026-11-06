namespace BookLending.Domain.Items
{
    public abstract class Magazine : LibraryItem
    {
        public override int LoanPeriod() => 7;
        protected Magazine(string title, string author, int year) : base(title, author, year)
        {
        }
    }
}