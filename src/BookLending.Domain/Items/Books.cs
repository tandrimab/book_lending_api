namespace BookLending.Domain.Items
{
    public class Book : LibraryItem
    {
        public string publisher { get; set; }
        public Book(string title, string author, int year, string publisher) : base(title, author, year)
        {
            this.publisher = publisher;
        }
    }
}