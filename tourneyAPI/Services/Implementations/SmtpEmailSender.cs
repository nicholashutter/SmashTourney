namespace Services;

using System.Net;
using System.Net.Mail;
using Entities;
using Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Serilog;

// Delivers account email through an SMTP relay.
//
// This replaces a sender that accepted every message and dropped it, which meant
// account confirmation and password reset were features the UI implied and the
// server never performed. Requiring a confirmed address is only meaningful once
// the confirmation can actually arrive.
public sealed class SmtpEmailSender : IEmailSender, IEmailSender<ApplicationUser>
{
    private readonly EmailOptions _emailOptions;

    public SmtpEmailSender(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    // Sends the account confirmation link.
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var (subject, body) = EmailMessageBuilder.BuildConfirmation(confirmationLink);
        return SendEmailAsync(email, subject, body);
    }

    // Sends a password reset code.
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var (subject, body) = EmailMessageBuilder.BuildPasswordResetCode(resetCode);
        return SendEmailAsync(email, subject, body);
    }

    // Sends a password reset link.
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var (subject, body) = EmailMessageBuilder.BuildPasswordResetLink(resetLink);
        return SendEmailAsync(email, subject, body);
    }

    // Sends one message through the configured relay.
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var smtpClient = new SmtpClient(_emailOptions.Smtp.Host, _emailOptions.Smtp.Port)
        {
            EnableSsl = _emailOptions.Smtp.UseStartTls,
            Timeout = _emailOptions.Smtp.TimeoutSeconds * 1000
        };

        // An unauthenticated relay is a normal local setup, so empty credentials
        // mean "do not authenticate" rather than "authenticate as nobody".
        if (!string.IsNullOrWhiteSpace(_emailOptions.Smtp.UserName))
        {
            smtpClient.Credentials = new NetworkCredential(
                _emailOptions.Smtp.UserName,
                _emailOptions.Smtp.Password);
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailOptions.FromAddress, _emailOptions.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);

            // The address is logged, never the body: confirmation and reset
            // bodies contain single-use credentials, and a log file is a much
            // easier thing to read than a mailbox.
            Log.Information("Sent {Subject} to {Recipient}", subject, email);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to send {Subject} to {Recipient}", subject, email);

            // Rethrown deliberately. Identity's register endpoint awaits this, and
            // silently swallowing a delivery failure would report a successful
            // sign-up to somebody who is never going to receive the one email
            // that lets them finish it.
            throw;
        }
    }
}
