using BookLending.Domain.Members;
namespace BookLending.Domain.Reminders
{
    public class SmsReminder : IReminderSender
    {
        public async Task SendReminderAsync(Member member, string message)
        {
            await Task.Delay(1000);
            Console.WriteLine($"Sending SMS to {member.Name} at {member.Address.Street}, {member.Address.City}, {member.Address.State}, {member.Address.ZipCode}: {message}");
        }
    }
}