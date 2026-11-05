namespace BookLending.Domain.Items
{
    public abstract class LibraryItem
    {
        private static int Isbn = 1000000000;
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }

        protected LibraryItem(string title, string author, int year)
        {
            Title = title;
            Author = author;
            Year = year;
        }
    }
}