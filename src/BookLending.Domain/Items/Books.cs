namespace BookLending.Domain.Items
{
    public record struct IsbnNumber(string Value);
    public class Book : LibraryItem
    {
        public IsbnNumber Isbn { get; private set; }
        public string Publisher { get; private set; }

        public override int LoanPeriod() => 14;
        public Book(string title, string author, int year, string publisher, IsbnNumber isbn) : base(title, author, year)
        {
            Publisher = publisher;
            Isbn = isbn;
        }
    }
}