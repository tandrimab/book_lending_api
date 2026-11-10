using BookLending.Domain.Members;
using BookLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        return _context.Members.AsNoTracking().ToList();
    }

    public Member? GetMemberById(string id)
    {
        return _context.Members.AsNoTracking().FirstOrDefault(m => m.Id == id);
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }


}