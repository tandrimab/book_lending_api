using BookLending.Domain.Members;
namespace BookLending.Domain.Reminders
{
    public class EmailReminder : IReminderSender
    {
        public async Task SendReminderAsync(Member member, string message)
        {
            await Task.Delay(1000);
            Console.WriteLine($"Sending email to {member.Name} at {member.Address.Street}, {member.Address.City}, {member.Address.State}, {member.Address.ZipCode}: {message}");
        }
    }
}