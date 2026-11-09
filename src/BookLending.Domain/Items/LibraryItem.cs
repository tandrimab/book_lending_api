namespace BookLending.Domain.Items
{
    public abstract class LibraryItem
    {
        public string Id { get; private set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int Year { get; set; }
        public virtual int LoanPeriod() => 7;

        // For EF Core: lets derived classes' parameterless constructors chain to a base constructor.
        protected LibraryItem() { }

        protected LibraryItem(string title, string author, int year)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Author = author;
            Year = year;
        }
    }
}
