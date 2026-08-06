namespace Routers;

using Entities;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Helpers;
using System.Security.Claims;
using System.Text;

// Maps user authentication and account management endpoints.
public static class UserRouter
{
    // Represents a username/password login payload.
    private sealed class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Represents a registration payload carrying a chosen display username.
    private sealed class RegisterAccountRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Builds the link that confirms an address.
    //
    // Pointed at Identity's own /confirmEmail endpoint, in the shape it expects:
    // the token is Base64Url-encoded because it travels in a query string and
    // raw tokens contain characters that do not survive that.
    //
    // The host comes from configuration when it is set, because behind a proxy or
    // a tunnel the request's own host is an internal name that no mail client can
    // reach — and a confirmation link nobody can open is the same as no email.
    private static string BuildConfirmationLink(
        HttpContext context,
        EmailOptions emailOptions,
        string userId,
        string token)
    {
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var baseUrl = string.IsNullOrWhiteSpace(emailOptions.PublicBaseUrl)
            ? $"{context.Request.Scheme}://{context.Request.Host}"
            : emailOptions.PublicBaseUrl.TrimEnd('/');

        return $"{baseUrl}/confirmEmail?userId={Uri.EscapeDataString(userId)}&code={encodedToken}";
    }

    // Registers all user API endpoints.
    public static void Map(WebApplication app)
    {
        // Closed by default, with the two routes that cannot require a session
        // opting out explicitly below. Sign-in obviously cannot demand proof of
        // sign-in, but that exemption belongs on the route that needs it rather
        // than being the default for everything under /users.
        var userRoutes = app.MapGroup("/users").RequireAuthorization();

        // Registers an account with a chosen username, and emails a confirmation.
        //
        // The client posted to /Users/Register, which never existed — no route
        // was ever mapped at that path, so every sign-up returned 404. It also
        // sent only a username and password, so the email address the form
        // collected was thrown away before the request left the browser.
        //
        // Identity's own /register would work but keys accounts on the address
        // alone, and this application has usernames: the login route accepts
        // either, and the seeded development accounts rely on it. So registration
        // sets both, then sends the confirmation Identity would have sent.
        userRoutes.MapPost("/register", async (
            RegisterAccountRequest registerRequest,
            UserManager<ApplicationUser> identityUserManager,
            Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser> emailSender,
            IOptions<EmailOptions> emailOptions,
            HttpContext context) =>
        {
            Log.Information("Request Type: Post \n URL: '/users/register' \n Time: {Timestamp}", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(registerRequest.UserName)
                || string.IsNullOrWhiteSpace(registerRequest.Email)
                || string.IsNullOrWhiteSpace(registerRequest.Password))
            {
                return Results.BadRequest(new { Message = "Username, email and password are all required." });
            }

            var newUser = new ApplicationUser
            {
                UserName = registerRequest.UserName.Trim(),
                Email = registerRequest.Email.Trim(),
                EmailConfirmed = false
            };

            var creationResult = await identityUserManager.CreateAsync(newUser, registerRequest.Password);

            if (!creationResult.Succeeded)
            {
                // Identity's own messages are returned because they are the only
                // thing that tells somebody *why* a password was refused. They
                // describe the submitted value, not any stored account, so this
                // does not disclose whether an address is already registered
                // beyond what Identity already says.
                var failureReasons = creationResult.Errors.Select(error => error.Description).ToArray();

                Log.Warning("Registration refused: {Reasons}", string.Join("; ", failureReasons));

                return Results.BadRequest(new { Message = "Could not create the account.", Reasons = failureReasons });
            }

            var confirmationToken = await identityUserManager.GenerateEmailConfirmationTokenAsync(newUser);
            var confirmationLink = BuildConfirmationLink(context, emailOptions.Value, newUser.Id, confirmationToken);

            try
            {
                await emailSender.SendConfirmationLinkAsync(newUser, newUser.Email!, confirmationLink);
            }
            catch (Exception exception)
            {
                // The account exists but its one route to being usable failed.
                // Deleting it again is what keeps the address free to try later,
                // rather than leaving a permanently unconfirmable account
                // squatting on it — RequireUniqueEmail would block the retry.
                Log.Error(exception, "Rolling back registration because the confirmation email could not be sent");

                await identityUserManager.DeleteAsync(newUser);

                return Results.Problem(
                    "Your account could not be created because we could not send the confirmation email. Please try again.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            return Results.Ok(new
            {
                Message = "Account created. Check your email for the confirmation link before signing in.",
                RequiresEmailConfirmation = true
            });
        })
        .AllowAnonymous()
        .RequireRateLimiting(AppConstants.AuthRateLimiterPolicy);

        userRoutes.MapPost("/login", async (
            LoginRequest loginRequest,
            UserManager<ApplicationUser> identityUserManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            Log.Information("Request Type: Post \n URL: '/users/login' \n Time: {Timestamp}", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return Results.BadRequest("Username and password are required.");
            }

            var foundUser = await identityUserManager.FindByNameAsync(loginRequest.UserName)
                ?? await identityUserManager.FindByEmailAsync(loginRequest.UserName);

            if (foundUser is null)
            {
                return Results.Unauthorized();
            }

            // Failures count towards lockout. This passed false, which meant a
            // public login form accepted unlimited password guesses against any
            // account for as long as somebody cared to keep trying.
            var signInResult = await signInManager.PasswordSignInAsync(
                foundUser,
                loginRequest.Password,
                isPersistent: true,
                lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                Log.Warning("Rejected sign-in for a locked out account");
                return Results.Json(
                    new { Message = "Too many failed attempts. Try again in a few minutes." },
                    statusCode: StatusCodes.Status423Locked);
            }

            if (signInResult.IsNotAllowed)
            {
                // Almost always an unconfirmed address. Said plainly, because
                // somebody who has registered and cannot get in needs to know to
                // go and look in their inbox.
                return Results.Json(
                    new { Message = "Confirm your email address before signing in." },
                    statusCode: StatusCodes.Status403Forbidden);
            }

            if (!signInResult.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { Message = "Login successful" });
        })
        .AllowAnonymous()
        .RequireRateLimiting(AppConstants.AuthRateLimiterPolicy);

        userRoutes.MapGet("/demo-credentials", (IHostEnvironment environment) =>
        {
            if (!environment.IsDevelopment())
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                UserName = AppConstants.DemoUserName,
                Password = AppConstants.DemoUserPassword
            });
        }).AllowAnonymous();

        userRoutes.MapGet("/session", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userName = user.Identity?.Name ?? string.Empty;

            return Results.Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                UserId = userId,
                UserName = userName
            });
        }).RequireAuthorization();

        userRoutes.MapPost("/logout", async (HttpContext context, IGameService gameService) =>
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);

                Log.Information("Request Type: Post \n URL: '/users/logout' \n Time: {Timestamp}", DateTime.UtcNow);

                bool success = gameService.EndUserSession(context.User);

                if (!success)
                {
                    return Results.Problem("Internal Server Error");
                }

                return Results.Ok();
            }).RequireAuthorization();
    }
}
