using LensmaniaServer.Services;

namespace LensmaniaTests.Helpers;

public class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string HtmlBody)> SentEmails { get;} = new();

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        SentEmails.Add((to, subject, htmlBody));
        return Task.CompletedTask;
    }
}