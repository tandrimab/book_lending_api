using BookLending.Domain.Members;
using BookLending.Domain.Reminders;
namespace BookLending.Application.Loans;

public class LoanCreatedHandler
{
    private readonly IMemberRepository _members;
    private readonly IReminderSender _reminders;

    public LoanCreatedHandler(IMemberRepository members, IReminderSender reminders)
    {
        _members = members;
        _reminders = reminders;
    }

    public async Task HandleAsync(LoanCreated message, CancellationToken cancellationToken = default)
    {
        var member = _members.GetMemberById(message.MemberId);

        if (member is null) return;

        await _reminders.SendReminderAsync(member,
            $"You borrowed a book (loan {message.LoanId}). Please return it by {message.DueDate:dd MMM yyyy}.");
    }
}
