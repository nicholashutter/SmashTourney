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

    // Generates one unique credential pair for isolated test users.
    private RegisterRequest CreateRandomCredentials()
    {
        return new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@email.com",
            Password = "SecureP@ssw0rd123!"
        };
    }

    // Registers and logs in one user and returns the authenticated client.
    private async Task<HttpClient> RegisterAndLoginAsync(RegisterRequest credentials, bool useCookies)
    {
        var client = NewClient(handleCookies: useCookies);

        var registerUrl = "/register";
        if (useCookies)
        {
            registerUrl = "/register?useCookies=true";
        }

        var loginUrl = "/login";
        if (useCookies)
        {
            loginUrl = "/login?useCookies=true";
        }

        var registerResponse = await client.PostAsJsonAsync(registerUrl, credentials);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync(loginUrl, credentials);
        loginResponse.EnsureSuccessStatusCode();

        return client;
    }

    // Confirms that register endpoint creates a new user account successfully.
    [Theory]
    [InlineData("/register")]
    public async Task RegisterNewUser(string url)
    {
        var client = NewClient();
        var credentials = CreateRandomCredentials();

        var response = await client.PostAsJsonAsync(url, credentials);

        Assert.True(response.IsSuccessStatusCode);
    }

    // Confirms that login flow succeeds after a user is registered.
    [Fact]
    public async Task LoginNewUser()
    {
        var credentials = CreateRandomCredentials();
        var client = await RegisterAndLoginAsync(credentials, useCookies: true);
        Assert.NotNull(client);
    }

    // Confirms that authenticated users can access secure endpoints requiring cookies.
    [Fact]
    public async Task SecureEndpointWithCookieReturnsOk()
    {
        var credentials = CreateRandomCredentials();
        var client = await RegisterAndLoginAsync(credentials, useCookies: true);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Confirms that secure endpoints reject users without an authenticated session.
    [Fact]
    public async Task SecureEndpointWithoutCookieReturnsUnauthorized()
    {
        var client = NewClient(handleCookies: true);

        var response = await client.GetAsync("/users/GetAllUsers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Confirms that authenticated users can end their session via logout route.
    [Fact]
    public async Task LogoutEndsSessionSuccessfully()
    {
        var credentials = CreateRandomCredentials();
        var client = await RegisterAndLoginAsync(credentials, useCookies: true);

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
        var credentials = CreateRandomCredentials();
        var client = await RegisterAndLoginAsync(credentials, useCookies: true);

        var response = await client.GetAsync("/users/session");

        Assert.True(response.IsSuccessStatusCode);
    }
}
