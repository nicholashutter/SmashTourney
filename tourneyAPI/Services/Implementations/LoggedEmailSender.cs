namespace Services;

using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;

// Writes account email to the log instead of sending it.
//
// Used when no SMTP relay is configured, which is the normal state of a developer
// machine. It exists so that requiring a confirmed address does not make local
// sign-up impossible: the confirmation link is printed where whoever is running
// the server can click it.
//
// This deliberately does not silently succeed the way the previous sender did.
// That one accepted every message and dropped it, so a missing relay looked
// exactly like a working one. Here the link is in the log and the warning says
// no mail was sent, so the difference is visible.
public sealed class LoggedEmailSender : IEmailSender, IEmailSender<ApplicationUser>
{
    // Logs the account confirmation link.
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        Log.Warning(
            "No SMTP relay configured. Confirmation link for {Recipient} was not emailed: {ConfirmationLink}",
            email,
            confirmationLink);

        return Task.CompletedTask;
    }

    // Logs a password reset code.
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        Log.Warning(
            "No SMTP relay configured. Password reset code for {Recipient} was not emailed: {ResetCode}",
            email,
            resetCode);

        return Task.CompletedTask;
    }

    // Logs a password reset link.
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        Log.Warning(
            "No SMTP relay configured. Password reset link for {Recipient} was not emailed: {ResetLink}",
            email,
            resetLink);

        return Task.CompletedTask;
    }

    // Logs a generic message without sending it.
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Log.Warning(
            "No SMTP relay configured. Message '{Subject}' for {Recipient} was not emailed.",
            subject,
            email);

        return Task.CompletedTask;
    }
}
