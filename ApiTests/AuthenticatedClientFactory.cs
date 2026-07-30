namespace ApiTests;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

// One registered, confirmed, signed-in account and the client carrying its cookie.
public sealed record AuthenticatedAccount(HttpClient Client, string UserName, string Email, string Password);

// Creates signed-in test clients by walking the whole account flow.
//
// Sign-in requires a confirmed address now, so every test that needs an
// authenticated caller has to register, receive the confirmation email, follow
// its link, and then sign in. That is a real sequence rather than a shortcut:
// each helper below goes through the API, so the flow the tests depend on is the
// flow a person gets.
//
// It matters that this does not confirm addresses via UserManager directly.
// Doing so would leave every test passing while registration silently stopped
// sending mail, which is precisely the failure the previous no-op sender hid.
public static class AuthenticatedClientFactory
{
    // Any password used here has to satisfy the twelve-character policy.
    public const string ValidPassword = "SecureP@ssw0rd123!";

    // Registers, confirms and signs in one account.
    public static async Task<AuthenticatedAccount> CreateAsync(
        CustomWebApplicationFactory<Program> factory,
        string namePrefix)
    {
        var account = await RegisterAsync(factory, namePrefix);

        await ConfirmAsync(factory, account.Client, account.Email);
        await SignInAsync(account.Client, account.UserName, account.Password);

        return account;
    }

    // Registers one account without confirming it.
    //
    // Kept separate so tests can assert what an unconfirmed account cannot do.
    public static async Task<AuthenticatedAccount> RegisterAsync(
        CustomWebApplicationFactory<Program> factory,
        string namePrefix)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var userName = $"{namePrefix}_{uniqueSuffix}";
        var email = $"{userName}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/users/register", new
        {
            userName,
            email,
            password = ValidPassword
        });

        registerResponse.EnsureSuccessStatusCode();

        return new AuthenticatedAccount(client, userName, email, ValidPassword);
    }

    // Follows the confirmation link out of the email the server actually sent.
    public static async Task ConfirmAsync(
        CustomWebApplicationFactory<Program> factory,
        HttpClient client,
        string email)
    {
        var confirmationEmail = factory.SentEmail.LatestFor(email);

        Assert.NotNull(confirmationEmail);
        Assert.NotNull(confirmationEmail!.Link);

        var confirmResponse = await client.GetAsync(ToRelativeUrl(confirmationEmail.Link!));
        confirmResponse.EnsureSuccessStatusCode();
    }

    // Signs in and leaves the auth cookie on the client.
    public static async Task SignInAsync(HttpClient client, string userName, string password)
    {
        var loginResponse = await client.PostAsJsonAsync("/users/login", new
        {
            userName,
            password
        });

        loginResponse.EnsureSuccessStatusCode();
    }

    // Reduces an absolute emailed link to a path the test client can request.
    //
    // The link is built for a mail client and carries a real host and scheme. The
    // test server only answers relative requests, so the path and query are what
    // survive the trip — which also means the query string is exercised exactly
    // as sent, token encoding included.
    public static string ToRelativeUrl(string absoluteOrRelativeUrl)
    {
        if (Uri.TryCreate(absoluteOrRelativeUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.PathAndQuery;
        }

        return absoluteOrRelativeUrl;
    }
}
