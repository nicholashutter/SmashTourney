namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Walks the account lifecycle the way a person does: register, receive the email,
// follow the link, sign in, sign out, reset a forgotten password.
//
// Every step goes over HTTP against the real endpoints, and every token comes out
// of an email the application genuinely produced. Nothing mints a confirmation
// token through UserManager, because a test that does so would keep passing if
// registration stopped sending mail entirely — which is exactly the state this
// application was in before, with a sender that accepted every message and
// dropped it.
public class AuthEndToEndTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthEndToEndTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    private HttpClient NewClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    // Confirms registering sends a confirmation email carrying a usable link.
    [Fact]
    public async Task RegisteringSendsAConfirmationEmail()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "confirm");

        var confirmationEmail = _factory.SentEmail.LatestFor(account.Email);

        Assert.NotNull(confirmationEmail);
        Assert.Contains("Confirm", confirmationEmail!.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(confirmationEmail.Link);
        Assert.Contains("/confirmEmail", confirmationEmail.Link!);
        Assert.Contains("code=", confirmationEmail.Link!);
    }

    // Confirms the address collected at registration is the one written to.
    //
    // The sign-up form used to collect an address and drop it before sending, so
    // an account could exist with no way to reach its owner.
    [Fact]
    public async Task TheConfirmationGoesToTheAddressThatRegistered()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "addressed");

        Assert.Equal(1, _factory.SentEmail.CountFor(account.Email));
    }

    // Confirms an unconfirmed account cannot sign in.
    //
    // This is the whole point of requiring confirmation: until it happens the
    // account is only a claim on somebody else's address.
    [Fact]
    public async Task AnUnconfirmedAccountCannotSignIn()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "unconfirmed");

        var loginResponse = await account.Client.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    // Confirms an unconfirmed account cannot reach a protected route either.
    [Fact]
    public async Task AnUnconfirmedAccountHasNoSession()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "nosession");

        var sessionResponse = await account.Client.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, sessionResponse.StatusCode);
    }

    // Confirms following the emailed link makes the account usable.
    [Fact]
    public async Task FollowingTheEmailedLinkAllowsSignIn()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "happy");

        await AuthenticatedClientFactory.ConfirmAsync(_factory, account.Client, account.Email);

        var loginResponse = await account.Client.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.True(loginResponse.IsSuccessStatusCode);

        var sessionResponse = await account.Client.GetAsync("/users/session");
        sessionResponse.EnsureSuccessStatusCode();
    }

    // Confirms signing in works with the address as well as the username.
    //
    // The login route accepts either, and registration now stores both, so this
    // pins down that the two really are interchangeable.
    [Fact]
    public async Task SigningInWorksWithTheEmailAddressToo()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "byemail");

        var freshClient = NewClient();

        var loginResponse = await freshClient.PostAsJsonAsync("/users/login", new
        {
            userName = account.Email,
            password = account.Password
        });

        Assert.True(loginResponse.IsSuccessStatusCode);
    }

    // Confirms a tampered confirmation token is refused.
    [Fact]
    public async Task ATamperedConfirmationTokenIsRefused()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "tampered");

        var confirmationEmail = _factory.SentEmail.LatestFor(account.Email);
        var tamperedLink = confirmationEmail!.Link!.Replace("code=", "code=zzzz");

        var confirmResponse = await account.Client.GetAsync(
            AuthenticatedClientFactory.ToRelativeUrl(tamperedLink));

        Assert.False(confirmResponse.IsSuccessStatusCode);

        // And the account is still unusable afterwards.
        var loginResponse = await account.Client.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    // Confirms one account's confirmation link does not confirm another account.
    [Fact]
    public async Task AConfirmationLinkOnlyConfirmsItsOwnAccount()
    {
        var firstAccount = await AuthenticatedClientFactory.RegisterAsync(_factory, "linkowner");
        var secondAccount = await AuthenticatedClientFactory.RegisterAsync(_factory, "linkthief");

        // The thief follows the owner's link.
        var ownersEmail = _factory.SentEmail.LatestFor(firstAccount.Email);
        await secondAccount.Client.GetAsync(
            AuthenticatedClientFactory.ToRelativeUrl(ownersEmail!.Link!));

        var thiefLoginResponse = await secondAccount.Client.PostAsJsonAsync("/users/login", new
        {
            userName = secondAccount.UserName,
            password = secondAccount.Password
        });

        Assert.Equal(HttpStatusCode.Forbidden, thiefLoginResponse.StatusCode);
    }

    // Confirms a wrong password is refused.
    [Fact]
    public async Task AWrongPasswordIsRefused()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "wrongpass");

        var freshClient = NewClient();

        var loginResponse = await freshClient.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = "TotallyWrongP@ss1"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    // Confirms repeated wrong passwords lock the account out.
    //
    // Sign-in passed lockoutOnFailure: false, so a public login form accepted
    // unlimited guesses against any account for as long as somebody cared to
    // keep trying. The lock is what makes a weak password survivable.
    [Fact]
    public async Task RepeatedWrongPasswordsLockTheAccount()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "lockout");

        var attackerClient = NewClient();

        HttpResponseMessage? lastResponse = null;

        // Five is the configured threshold; the sixth attempt meets the lock.
        for (var attempt = 0; attempt < 6; attempt++)
        {
            lastResponse = await attackerClient.PostAsJsonAsync("/users/login", new
            {
                userName = account.UserName,
                password = $"WrongGuess{attempt}!aB"
            });
        }

        Assert.Equal(HttpStatusCode.Locked, lastResponse!.StatusCode);
    }

    // Confirms a locked account refuses even the correct password.
    //
    // Otherwise the lock would only slow down a guesser who never happened to
    // guess right, which is no protection at all.
    [Fact]
    public async Task ALockedAccountRefusesTheCorrectPassword()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "lockedright");

        var attackerClient = NewClient();

        for (var attempt = 0; attempt < 6; attempt++)
        {
            await attackerClient.PostAsJsonAsync("/users/login", new
            {
                userName = account.UserName,
                password = $"WrongGuess{attempt}!aB"
            });
        }

        var honestClient = NewClient();
        var loginResponse = await honestClient.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.Equal(HttpStatusCode.Locked, loginResponse.StatusCode);
    }

    // Confirms guessing one account does not lock a different one.
    [Fact]
    public async Task LockingOneAccountDoesNotLockAnother()
    {
        var targetAccount = await AuthenticatedClientFactory.CreateAsync(_factory, "locktarget");
        var bystanderAccount = await AuthenticatedClientFactory.CreateAsync(_factory, "bystander");

        var attackerClient = NewClient();

        for (var attempt = 0; attempt < 6; attempt++)
        {
            await attackerClient.PostAsJsonAsync("/users/login", new
            {
                userName = targetAccount.UserName,
                password = $"WrongGuess{attempt}!aB"
            });
        }

        var bystanderClient = NewClient();
        var loginResponse = await bystanderClient.PostAsJsonAsync("/users/login", new
        {
            userName = bystanderAccount.UserName,
            password = bystanderAccount.Password
        });

        Assert.True(loginResponse.IsSuccessStatusCode);
    }

    // Confirms registration refuses a password below the length policy.
    [Fact]
    public async Task RegistrationRefusesAShortPassword()
    {
        var client = NewClient();

        var registerResponse = await client.PostAsJsonAsync("/users/register", new
        {
            userName = $"shortpass_{Guid.NewGuid():N}",
            email = $"shortpass_{Guid.NewGuid():N}@example.com",
            password = "Ab1!short"
        });

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    // Confirms registration refuses a request with no address.
    //
    // The form used to omit the address entirely, so this is the case that would
    // have slipped through as a valid sign-up.
    [Fact]
    public async Task RegistrationRefusesAMissingEmailAddress()
    {
        var client = NewClient();

        var registerResponse = await client.PostAsJsonAsync("/users/register", new
        {
            userName = $"noemail_{Guid.NewGuid():N}",
            email = "",
            password = AuthenticatedClientFactory.ValidPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    // Confirms two accounts cannot share one address.
    [Fact]
    public async Task TwoAccountsCannotShareAnEmailAddress()
    {
        var firstAccount = await AuthenticatedClientFactory.RegisterAsync(_factory, "dupemail");

        var client = NewClient();
        var registerResponse = await client.PostAsJsonAsync("/users/register", new
        {
            userName = $"other_{Guid.NewGuid():N}",
            email = firstAccount.Email,
            password = AuthenticatedClientFactory.ValidPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    // Confirms signing out ends the session.
    [Fact]
    public async Task SigningOutEndsTheSession()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "signout");

        (await account.Client.GetAsync("/users/session")).EnsureSuccessStatusCode();

        var logoutResponse = await account.Client.PostAsync("/users/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        var afterResponse = await account.Client.GetAsync("/users/session");
        Assert.Equal(HttpStatusCode.Unauthorized, afterResponse.StatusCode);
    }

    // Confirms a forgotten password can be reset through the emailed code.
    [Fact]
    public async Task AForgottenPasswordCanBeResetFromTheEmail()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "reset");

        var forgotResponse = await account.Client.PostAsJsonAsync("/forgotPassword", new
        {
            email = account.Email
        });

        forgotResponse.EnsureSuccessStatusCode();

        var resetEmail = _factory.SentEmail.LatestFor(account.Email);
        Assert.NotNull(resetEmail);
        Assert.NotNull(resetEmail!.Code);

        const string newPassword = "BrandNewP@ssw0rd99!";

        var resetResponse = await account.Client.PostAsJsonAsync("/resetPassword", new
        {
            email = account.Email,
            resetCode = resetEmail.Code,
            newPassword
        });

        resetResponse.EnsureSuccessStatusCode();

        // The new password works.
        var newClient = NewClient();
        var newLoginResponse = await newClient.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = newPassword
        });

        Assert.True(newLoginResponse.IsSuccessStatusCode);
    }

    // Confirms the old password stops working after a reset.
    //
    // A reset that leaves the previous password valid has not taken anything away
    // from whoever prompted the reset.
    [Fact]
    public async Task TheOldPasswordStopsWorkingAfterAReset()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "resetold");

        (await account.Client.PostAsJsonAsync("/forgotPassword", new { email = account.Email }))
            .EnsureSuccessStatusCode();

        var resetEmail = _factory.SentEmail.LatestFor(account.Email);

        (await account.Client.PostAsJsonAsync("/resetPassword", new
        {
            email = account.Email,
            resetCode = resetEmail!.Code,
            newPassword = "AnotherNewP@ssw0rd77!"
        })).EnsureSuccessStatusCode();

        var oldPasswordClient = NewClient();
        var oldLoginResponse = await oldPasswordClient.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldLoginResponse.StatusCode);
    }

    // Confirms a reset code cannot be replayed.
    [Fact]
    public async Task AResetCodeCannotBeUsedTwice()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "replay");

        (await account.Client.PostAsJsonAsync("/forgotPassword", new { email = account.Email }))
            .EnsureSuccessStatusCode();

        var resetEmail = _factory.SentEmail.LatestFor(account.Email);

        (await account.Client.PostAsJsonAsync("/resetPassword", new
        {
            email = account.Email,
            resetCode = resetEmail!.Code,
            newPassword = "FirstReplacementP@ss1!"
        })).EnsureSuccessStatusCode();

        var secondUseResponse = await account.Client.PostAsJsonAsync("/resetPassword", new
        {
            email = account.Email,
            resetCode = resetEmail.Code,
            newPassword = "SecondReplacementP@ss2!"
        });

        Assert.False(secondUseResponse.IsSuccessStatusCode);
    }

    // Confirms asking to reset an unknown address does not disclose that.
    //
    // Answering differently for a registered and an unregistered address turns the
    // reset form into a way to test whether somebody has an account here.
    [Fact]
    public async Task ResettingAnUnknownAddressLooksTheSameAsAKnownOne()
    {
        var knownAccount = await AuthenticatedClientFactory.CreateAsync(_factory, "known");

        var knownResponse = await NewClient().PostAsJsonAsync("/forgotPassword", new
        {
            email = knownAccount.Email
        });

        var unknownResponse = await NewClient().PostAsJsonAsync("/forgotPassword", new
        {
            email = $"nobody_{Guid.NewGuid():N}@example.com"
        });

        Assert.Equal(knownResponse.StatusCode, unknownResponse.StatusCode);
    }

    // Confirms a confirmation email can be sent again.
    //
    // The first one gets lost, filtered, or deleted, and without this the account
    // would be permanently stuck: unable to sign in, and unable to ask again.
    [Fact]
    public async Task AConfirmationEmailCanBeResent()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "resend");

        var resendResponse = await account.Client.PostAsJsonAsync("/resendConfirmationEmail", new
        {
            email = account.Email
        });

        resendResponse.EnsureSuccessStatusCode();

        Assert.True(_factory.SentEmail.CountFor(account.Email) >= 2);

        // And the newest link works.
        await AuthenticatedClientFactory.ConfirmAsync(_factory, account.Client, account.Email);

        var loginResponse = await account.Client.PostAsJsonAsync("/users/login", new
        {
            userName = account.UserName,
            password = account.Password
        });

        Assert.True(loginResponse.IsSuccessStatusCode);
    }

    // Confirms a signed-in account can reach game routes.
    //
    // Ties the auth flow to the thing it exists for: the end of registration is
    // being able to run a tournament.
    [Fact]
    public async Task AConfirmedAccountCanCreateATournament()
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, "playing");

        var createResponse = await account.Client.PostAsJsonAsync("/Games/CreateGameWithMode", new
        {
            bracketMode = "SINGLE_ELIMINATION"
        });

        createResponse.EnsureSuccessStatusCode();
    }
}
