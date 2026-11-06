namespace BookLending.Domain.Members
{
    public record Address(string Street, string City, string State, string ZipCode);
    public class Member
    {
        private static int _idCounter = 1;
        public string Id { get; private set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public DateTime MembershipStartDate { get; set; }

        protected Member(string name, Address address, DateTime membershipStartDate)
        {
            Id = $"member_{_idCounter++}";
            Name = name;
            Address = address;
            MembershipStartDate = membershipStartDate;
        }
    }
}