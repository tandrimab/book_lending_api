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

/*
* task is the foundation of Async programming in C#. It denotes that a method is running running async and will complete in future. It lives in System.Threading.Tasks namespace. Task task means a task with no value type, where Task <int> means a task that returns int.
Exaple
*/

// public async Task<string> GetUserDetails()
// {
//     var response = await httpClient.GetAsync("https://api.example.com/userdetails");
//     var json = await response.Content.ReadAsStringAsync();
//     return json;
// }
