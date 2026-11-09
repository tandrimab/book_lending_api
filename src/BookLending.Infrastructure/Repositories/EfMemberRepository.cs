using BookLending.Domain.Members;
using BookLending.Infrastructure.Persistence;
namespace BookLending.Infrastructure.Repositories;

public class EfMemberRepository : IMemberRepository
{
    private readonly BookLendingDbContext _context;

    public EfMemberRepository(BookLendingDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Member> GetAllMembers()
    {
        return _context.Members.ToList();
    }

    public Member? GetMemberById(string id)
    {
        return _context.Members.Find(id);
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }


}