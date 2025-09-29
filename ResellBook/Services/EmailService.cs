using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace ResellBook.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Resell Panda", _config["SMTP:User"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient(); // MailKit's SmtpClient
            await client.ConnectAsync(_config["SMTP:Host"], int.Parse(_config["SMTP:Port"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["SMTP:User"], _config["SMTP:Pass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
