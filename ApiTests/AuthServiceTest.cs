namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using ApiTests.TestContracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies business-facing authentication and session behavior for signed-in and signed-out users.
public class AuthServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    // Initializes test host resources required by authentication route tests.
    public AuthServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Creates a test client and optionally preserves auth cookies.
    private HttpClient NewClient(bool handleCookies = false)
    {
        var options = new WebApplicationFactoryClientOptions
        {
            HandleCookies = handleCookies
        };

        return _factory.CreateClient(options);
    }

    // Registers, confirms and signs in one user, returning the ready client.
    //
    // Registering and logging in is no longer sufficient: sign-in requires a
    // confirmed address, so this goes through the real confirmation email.
    private async Task<HttpClient> RegisterAndLoginAsync(string namePrefix)
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, namePrefix);
        return account.Client;
    }

    // Confirms that the registration endpoint creates a new user account successfully.
    [Fact]
    public async Task RegisterNewUser()
    {
        var account = await AuthenticatedClientFactory.RegisterAsync(_factory, "register");

        Assert.NotNull(_factory.SentEmail.LatestFor(account.Email));
    }

    // Confirms that login flow succeeds after a user is registered and confirmed.
    [Fact]
    public async Task LoginNewUser()
    {
        var client = await RegisterAndLoginAsync("login");
        Assert.NotNull(client);
    }

    // Confirms that authenticated users can access secure endpoints requiring cookies.
    [Fact]
    public async Task SecureEndpointWithCookieReturnsOk()
    {
        var client = await RegisterAndLoginAsync("secure");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Confirms that secure endpoints reject users without an authenticated session.
    [Fact]
    public async Task SecureEndpointWithoutCookieReturnsUnauthorized()
    {
        var client = NewClient(handleCookies: true);

        var response = await client.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Confirms that authenticated users can end their session via logout route.
    [Fact]
    public async Task LogoutEndsSessionSuccessfully()
    {
        var client = await RegisterAndLoginAsync("logout");

        var logoutResponse = await client.PostAsync("/users/logout", null);
        Assert.True(logoutResponse.IsSuccessStatusCode);
    }

    // Confirms that session route requires authentication.
    [Fact]
    public async Task SessionEndpointRequiresAuthentication()
    {
        var client = NewClient(handleCookies: true);

        var response = await client.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Confirms that session route returns success for authenticated users.
    [Fact]
    public async Task SessionEndpointReturnsOkWhenAuthenticated()
    {
        var client = await RegisterAndLoginAsync("session");

        var response = await client.GetAsync("/users/session");

        Assert.True(response.IsSuccessStatusCode);
    }
}
