namespace BookLending.Domain.Items
{
    public class Magazine : LibraryItem
    {
        // Protected, not private: Magazine is abstract, so only subclasses can ever call it.
        protected Magazine() { }
        public override int LoanPeriod() => 7;
        public Magazine(string title, string author, int year) : base(title, author, year)
        {
        }
    }
}
