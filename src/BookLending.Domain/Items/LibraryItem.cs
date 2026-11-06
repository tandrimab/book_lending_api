namespace BookLending.Domain.Items
{
    public abstract class LibraryItem
    {
        private static int _idCounter = 0;
        private string Id;
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public virtual int LoanPeriod() => 7;

        protected LibraryItem(string title, string author, int year)
        {
            this.Id = (++_idCounter).ToString();
            Title = title;
            Author = author;
            Year = year;
        }
    }
}