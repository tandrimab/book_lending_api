using BookLending.Domain.Members;
using BookLending.Domain.Reminders;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MailKit;

namespace BookLending.Infrastructure.Email
{
    public  class SmtpEmailReminder : IReminderSender
    {
        private readonly SmtpSettings _s;
        public SmtpEmailReminder(SmtpSettings s)
        {
            _s = s;
        }
        public async Task SendReminderAsync(Member member, string message)
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress(_s.FromName, _s.FromAddress));
            mail.To.Add(new MailboxAddress(member.Name, member.Email));
            mail.Subject = "Book Lending notification";
            mail.Body = new TextPart("plain") { Text = message };
            using var client = new SmtpClient();
            await client.ConnectAsync(_s.Host, _s.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_s.Username, _s.Password);
            await client.SendAsync(new SendRequest(mail));
            await client.DisconnectAsync(true);
        }
    }
}
