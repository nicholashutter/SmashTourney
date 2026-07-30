namespace Helpers;

// Configures outbound email.
//
// Nothing here has a usable default and none of it belongs in source. The host
// and credentials come from configuration — environment variables or user
// secrets in development — because a committed SMTP password is a mail relay
// somebody else gets to use.
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    // Address recipients see, and reply to.
    public string FromAddress { get; set; } = string.Empty;

    // Display name shown alongside the from address.
    public string FromName { get; set; } = "Smash Tourney";

    // Base URL that confirmation and reset links point at.
    //
    // Identity builds these links from the incoming request by default, which is
    // wrong the moment the app sits behind a proxy or a tunnel: the link would
    // carry an internal host nobody outside can reach. Setting this pins them to
    // the address people actually use.
    public string PublicBaseUrl { get; set; } = string.Empty;

    public SmtpOptions Smtp { get; set; } = new();

    // Reports whether enough is configured to actually send mail.
    public bool IsDeliveryConfigured()
    {
        return !string.IsNullOrWhiteSpace(Smtp.Host)
            && !string.IsNullOrWhiteSpace(FromAddress);
    }
}

// Configures the SMTP relay used for delivery.
public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    // STARTTLS on the submission port, which is what nearly every relay wants.
    // Off only for a local capture tool that speaks plain SMTP.
    public bool UseStartTls { get; set; } = true;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    // How long to wait on the relay before giving up, in seconds.
    //
    // Registration waits on this, so it cannot be generous: a relay that has
    // gone away must not hold a sign-up open until the browser times out.
    public int TimeoutSeconds { get; set; } = 10;
}
