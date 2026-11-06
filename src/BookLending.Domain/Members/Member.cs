namespace BookLending.Domain.Members
{
    public record Address(string Street, string City, string State, string ZipCode);
    public class Member
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public DateTime MembershipStartDate { get; set; }

        protected Member(string name, Address address, DateTime membershipStartDate)
        {
            Name = name;
            Address = address;
            MembershipStartDate = membershipStartDate;
        }
    }
}