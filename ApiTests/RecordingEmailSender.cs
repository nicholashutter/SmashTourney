namespace ApiTests;

using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

// One captured email.
public sealed record CapturedEmail(string Recipient, string Subject, string Body, string? Link, string? Code);

// Captures account email instead of sending it, so tests can read what a real
// user would have received.
//
// This is what makes the auth tests end to end rather than a simulation. Nothing
// reaches into UserManager to mint a confirmation token behind the server's back;
// the test registers, waits for the email the application actually produced, digs
// the link out of the body, and follows it exactly as somebody would from their
// inbox. If registration ever stopped sending, or sent a link pointing at the
// wrong place, these tests would fail rather than quietly passing.
public sealed class RecordingEmailSender : IEmailSender, IEmailSender<ApplicationUser>
{
    private readonly ConcurrentQueue<CapturedEmail> _sentEmails = new();

    // Every email captured so far, oldest first.
    public IReadOnlyList<CapturedEmail> SentEmails => _sentEmails.ToList();

    // Returns the most recent email sent to one address, or null when there is none.
    //
    // Newest first because a test that resends a confirmation cares about the
    // second link, not the first.
    public CapturedEmail? LatestFor(string recipient)
    {
        return _sentEmails
            .Where(email => string.Equals(email.Recipient, recipient, StringComparison.OrdinalIgnoreCase))
            .LastOrDefault();
    }

    // How many emails have gone to one address.
    public int CountFor(string recipient)
    {
        return _sentEmails.Count(email =>
            string.Equals(email.Recipient, recipient, StringComparison.OrdinalIgnoreCase));
    }

    // Forgets everything captured so far.
    public void Clear()
    {
        _sentEmails.Clear();
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        Capture(email, "Confirm your Smash Tourney account", confirmationLink, AsClickableLink(confirmationLink), null);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        Capture(email, "Reset your Smash Tourney password", resetLink, AsClickableLink(resetLink), null);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        Capture(email, "Your Smash Tourney password reset code", resetCode, null, resetCode);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Capture(email, subject, htmlMessage, ExtractLink(htmlMessage), null);
        return Task.CompletedTask;
    }

    private void Capture(string recipient, string subject, string body, string? link, string? code)
    {
        _sentEmails.Enqueue(new CapturedEmail(recipient, subject, body, link, code));
    }

    // Pulls the first href out of an HTML body.
    private static string? ExtractLink(string body)
    {
        var match = Regex.Match(body, "href=\"(?<link>[^\"]+)\"");
        return match.Success ? AsClickableLink(match.Groups["link"].Value) : null;
    }

    // Resolves HTML entities the way a mail client does before navigating.
    //
    // Identity passes confirmation and reset URLs to the sender already
    // HTML-encoded, so the raw string contains "&amp;" where a browser would send
    // "&". Following it verbatim drops every query parameter after the first —
    // which is how the confirmation code went missing and produced a 400. Tests
    // have to navigate what a person would actually navigate.
    private static string AsClickableLink(string link)
    {
        return WebUtility.HtmlDecode(link);
    }
}
