using System.Collections.Concurrent;
using BookLending.Domain.Members;

namespace BookLending.Api.Repositories;

public class InMemoryMemberRepository : IMemberRepository
{
    private readonly ConcurrentDictionary<string, Member> _members = new();

    public IEnumerable<Member> GetAllMembers() => _members.Values;

    public Member? GetMemberById(string id) => _members.GetValueOrDefault(id);

    public void AddMember(Member member)
    {
        if (!_members.TryAdd(member.Id, member))
            throw new InvalidOperationException($"Member with id {member.Id} already exists.");
    }
}
