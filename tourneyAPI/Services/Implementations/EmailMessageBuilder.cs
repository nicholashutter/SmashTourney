namespace Services;

using System.Net;
using System.Text;

// Builds the bodies of the few emails this application sends.
//
// Kept apart from delivery so the wording can be read and changed without
// touching SMTP, and so both senders — the real one and the development one that
// logs — produce identical text.
public static class EmailMessageBuilder
{
    // Builds the account confirmation email.
    public static (string Subject, string HtmlBody) BuildConfirmation(string confirmationLink)
    {
        var subject = "Confirm your Smash Tourney account";

        var body = BuildHtml(
            "Confirm your account",
            "Tap the button to confirm this address and finish setting up your account.",
            "Confirm my account",
            confirmationLink,
            "If you did not create a Smash Tourney account, you can ignore this email and nothing will happen.");

        return (subject, body);
    }

    // Builds the password reset email carrying a link.
    public static (string Subject, string HtmlBody) BuildPasswordResetLink(string resetLink)
    {
        var subject = "Reset your Smash Tourney password";

        var body = BuildHtml(
            "Reset your password",
            "Tap the button to choose a new password.",
            "Choose a new password",
            resetLink,
            "If you did not ask to reset your password, you can ignore this email. Your current password still works.");

        return (subject, body);
    }

    // Builds the password reset email carrying a code rather than a link.
    public static (string Subject, string HtmlBody) BuildPasswordResetCode(string resetCode)
    {
        var subject = "Your Smash Tourney password reset code";

        var body = new StringBuilder()
            .Append("<!doctype html><html><body style=\"font-family:Arial,Helvetica,sans-serif;\">")
            .Append("<h2>Reset your password</h2>")
            .Append("<p>Use this code to choose a new password:</p>")
            .Append($"<p style=\"font-size:1.5rem;font-weight:bold;letter-spacing:0.1em;\">{WebUtility.HtmlEncode(resetCode)}</p>")
            .Append("<p style=\"color:#666;font-size:0.85rem;\">If you did not ask to reset your password, you can ignore this email. Your current password still works.</p>")
            .Append("</body></html>")
            .ToString();

        return (subject, body);
    }

    // Renders the shared single-action email layout.
    //
    // The link is repeated as text under the button because a fair number of mail
    // clients will not render the button as clickable, and a confirmation email
    // that cannot be actioned is the same as one that never arrived.
    private static string BuildHtml(
        string heading,
        string leadParagraph,
        string actionLabel,
        string actionUrl,
        string footnote)
    {
        // Decoded before encoding, so the URL is encoded exactly once.
        //
        // Identity hands these links over already HTML-encoded, and this used to
        // encode again: "&code=" became "&amp;amp;code=", which a mail client
        // resolves to a literal "&amp;code=" — so the code arrived as part of the
        // parameter *name* and confirmation failed with a 400. Links built
        // elsewhere in this application arrive raw. Decoding first makes both
        // inputs land in the same place.
        var encodedUrl = WebUtility.HtmlEncode(WebUtility.HtmlDecode(actionUrl));

        return new StringBuilder()
            .Append("<!doctype html><html><body style=\"font-family:Arial,Helvetica,sans-serif;line-height:1.5;\">")
            .Append($"<h2>{WebUtility.HtmlEncode(heading)}</h2>")
            .Append($"<p>{WebUtility.HtmlEncode(leadParagraph)}</p>")
            .Append($"<p><a href=\"{encodedUrl}\" style=\"display:inline-block;padding:0.6rem 1.2rem;background:#c8102e;color:#fff;text-decoration:none;border-radius:4px;font-weight:bold;\">{WebUtility.HtmlEncode(actionLabel)}</a></p>")
            .Append($"<p style=\"font-size:0.85rem;\">Or paste this into your browser:<br><span style=\"word-break:break-all;\">{encodedUrl}</span></p>")
            .Append($"<p style=\"color:#666;font-size:0.85rem;\">{WebUtility.HtmlEncode(footnote)}</p>")
            .Append("</body></html>")
            .ToString();
    }
}
