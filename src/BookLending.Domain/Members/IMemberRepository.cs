namespace BookLending.Domain.Members
{
    public interface IMemberRepository
    {
        IEnumerable<Member> GetAllMembers();
        Member? GetMemberById(string id);
        void AddMember(Member member);
    }
}
