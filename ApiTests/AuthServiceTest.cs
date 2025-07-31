namespace ApiTests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Services;
using Entities;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ApiTests;

public class AuthServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;



    public AuthServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [Theory]
    [InlineData("/register")]
    public async Task registerNewUser(string url)
    {
        var client = _factory.CreateClient();

        var req = new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@email.com",
            Password = "SecureP@ssw0rd123!"
        };

        var response = await client.PostAsJsonAsync(url, req);

        response.EnsureSuccessStatusCode();

    }

    [Fact]
    public async Task loginNewUser()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var req = new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@email.com",
            Password = "SecureP@ssw0rd123!"
        };

        string registerURL = "/register";
        var response = await client.PostAsJsonAsync(registerURL, req);

        string loginURL = "/login";
        response = await client.PostAsJsonAsync(loginURL, req);

        response.EnsureSuccessStatusCode();

    }

    [Fact]
    public async Task testSecureEndpointWithCookie()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        Guid gameId = await gameService.CreateGame();

        var email = $"test{Guid.NewGuid()}@email.com";
        var password = "SecureP@ssw0rd123!";
        var userName = email;

        var newUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            RegistrationDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow,
            AllTimeMatches = 0,
            AllTimeWins = 0,
            AllTimeLosses = 0
        };

        var result = await userManager.CreateUserAsync(newUser, password);

        Assert.True(result.Succeeded);

        var loginReq = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", loginReq);
        loginResponse.EnsureSuccessStatusCode();

        var afterLoginResponse = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, afterLoginResponse.StatusCode);
    }


    [Fact]
    public async Task testSecureEndpointNoCookie()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var requestNoCookie = await client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.Unauthorized, requestNoCookie.StatusCode);
    }


    [Fact]
    public async Task testLogout()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

         using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        Guid gameId = await gameService.CreateGame();

        var email = $"test{Guid.NewGuid()}@email.com";
        var password = "SecureP@ssw0rd123!";
        var userName = email;

        var newUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            RegistrationDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow,
            AllTimeMatches = 0,
            AllTimeWins = 0,
            AllTimeLosses = 0
        };

        var result = await userManager.CreateUserAsync(newUser, password);

        Assert.True(result.Succeeded);


        var req = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        

        var registerResponse = await client.PostAsJsonAsync("/register", req);

        var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", req);

        loginResponse.EnsureSuccessStatusCode();

        var afterLoginResponse = await client.GetAsync("/");

        afterLoginResponse.EnsureSuccessStatusCode();

        var logoutResponse = await client.PostAsync("/users/logout", null);

        logoutResponse.EnsureSuccessStatusCode();
    }


}