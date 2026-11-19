namespace BookLending.Infrastructure.Email
{
    public record SmtpSettings(
     string Host,
     int Port,
     string Username,
     string Password,
     string FromAddress,
     string FromName = "Book Lending");

}
