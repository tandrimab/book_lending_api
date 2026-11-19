namespace BookLending.Domain.Members
{
    public record Address(string Street, string City, string State, string ZipCode);
    public class Member
    {
        private Member() { }
        public string Id { get; private set; } = null!;
        public string Name { get; set; } = null!;
        public Address Address { get; set; } = null!;
        public DateTime MembershipStartDate { get; set; }

        public string? Email { get; set; }

        public Member(string name, Address address, DateTime membershipStartDate, string? email)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Address = address;
            MembershipStartDate = membershipStartDate;
            Email = email;
        }
    }
}
