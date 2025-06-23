
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;


namespace Tests;

public class AuthServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthServiceTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
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

    public async Task logoutUser()
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

        var registerResponse = await client.PostAsJsonAsync("/register", req);

        var loginResponse = await client.PostAsJsonAsync("/login", req);

        loginResponse.EnsureSuccessStatusCode();

        var success = await client.PostAsync("users/logout", null);

        success.EnsureSuccessStatusCode();

        var afterLogoutResponse = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, afterLogoutResponse.StatusCode);
    }



}