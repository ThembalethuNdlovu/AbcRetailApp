using Microsoft.AspNetCore.Identity.UI.Services;

namespace AbcRetailApp.Services
{
    // A placeholder email sender — this project doesn't send real emails,
    // but Identity's scaffolded Register page expects an IEmailSender to be registered.
    // This just logs to the console instead of actually sending anything.
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[NoOpEmailSender] Would send email to {email}: {subject}");
            return Task.CompletedTask;
        }
    }
}