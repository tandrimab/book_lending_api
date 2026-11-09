using BookLending.Domain.Members;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Api.Controllers;

public record CreateMemberRequest(string Name, Address Address);

[ApiController]
[Route("members")]
public class MembersController : ControllerBase
{
    private readonly IMemberRepository _members;

    public MembersController(IMemberRepository members) => _members = members;

    [HttpGet]
    public IActionResult GetAll() => Ok(_members.GetAllMembers());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var member = _members.GetMemberById(id);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost]
    public IActionResult Create(CreateMemberRequest request)
    {
        var member = new Member(request.Name, request.Address, DateTime.Now);
        _members.AddMember(member);

        return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
    }
}
