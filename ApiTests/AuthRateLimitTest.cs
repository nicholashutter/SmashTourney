namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies the auth endpoints stop answering under a flood.
//
// The shared test factory raises the limit out of the way, because a suite that
// registers hundreds of accounts a minute looks exactly like the attack this is
// meant to stop. So the limit is tested here, on a host that keeps a real one, and
// nowhere else.
//
// Lockout and rate limiting solve different halves of the same problem: lockout
// protects one account from many guesses, and this protects every account from one
// password tried across all of them. Neither substitutes for the other.
public class AuthRateLimitTest : IClassFixture<AuthRateLimitTest.RateLimitedFactory>
{
    // A test host that keeps a genuinely low rate limit.
    public sealed class RateLimitedFactory : CustomWebApplicationFactory<Program>
    {
        // Must match Auth:RateLimit:PermitLimit in appsettings.RateLimitTesting.json.
        public const int PermitLimit = 3;

        // A separate environment, so the low limit arrives through an appsettings
        // file. Program.cs reads the limit while building the host, before this
        // factory's configuration callbacks run, so a file is the only source
        // early enough to matter.
        protected override string EnvironmentName => "RateLimitTesting";
    }

    private readonly RateLimitedFactory _factory;

    public AuthRateLimitTest(RateLimitedFactory factory)
    {
        _factory = factory;

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Confirms a flood of sign-in attempts is eventually refused outright.
    //
    // Answered 429 rather than 401: the point is that the server stops doing the
    // work of checking at all, so guessing cannot be parallelised into an
    // unlimited rate against many accounts.
    [Fact]
    public async Task FloodingSignInIsRateLimited()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var sawTooManyRequests = false;

        // Comfortably past the limit, so the test does not depend on the exact
        // request the window closes on.
        for (var attempt = 0; attempt < RateLimitedFactory.PermitLimit + 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/users/login", new
            {
                userName = $"nobody_{attempt}",
                password = "IrrelevantP@ssw0rd1!"
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                sawTooManyRequests = true;
                break;
            }
        }

        Assert.True(sawTooManyRequests);
    }

    // Confirms registration is rate limited too.
    //
    // Otherwise the sign-up form is a way to make the server send mail to any
    // address somebody names, as fast as they can ask.
    [Fact]
    public async Task FloodingRegistrationIsRateLimited()
    {
        var client = _factory.CreateClient();

        var sawTooManyRequests = false;

        for (var attempt = 0; attempt < RateLimitedFactory.PermitLimit + 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/users/register", new
            {
                userName = $"flood_{Guid.NewGuid():N}",
                email = $"flood_{Guid.NewGuid():N}@example.com",
                password = AuthenticatedClientFactory.ValidPassword
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                sawTooManyRequests = true;
                break;
            }
        }

        Assert.True(sawTooManyRequests);
    }
}
