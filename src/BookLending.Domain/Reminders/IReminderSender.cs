using BookLending.Domain.Members;

namespace BookLending.Domain.Reminders
{
    public interface IReminderSender
    {
        Task SendReminderAsync(Member member, string message);
    }
}