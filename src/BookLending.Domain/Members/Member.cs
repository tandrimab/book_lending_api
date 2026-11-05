namespace BookLending.Domain.Members
{
    public record Address(string Street, string City, string State, string ZipCode);
    public abstract class Member
    {
        private static int MemberId = Guid.NewGuid().GetHashCode();

        public int Id { get; private set; } = MemberId;
        public string Name { get; set; }
        public Address Address { get; set; }
        public int Year { get; set; }

        protected Member(string name, Address address, int year)
        {
            MemberId++;
            Name = name;
            Address = address;
            Year = year;
        }
    }
}