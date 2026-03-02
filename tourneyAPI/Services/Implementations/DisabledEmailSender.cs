namespace Services;

using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

// Provides a no-delivery email sender while email infrastructure is disabled.
public sealed class DisabledEmailSender : IEmailSender, IEmailSender<ApplicationUser>
{
    // Accepts generic email requests without sending external email.
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }

    // Accepts confirmation link requests without sending external email.
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        return Task.CompletedTask;
    }

    // Accepts password reset code requests without sending external email.
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        return Task.CompletedTask;
    }

    // Accepts password reset link requests without sending external email.
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        return Task.CompletedTask;
    }
}
