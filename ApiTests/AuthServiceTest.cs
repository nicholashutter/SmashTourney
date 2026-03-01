namespace ApiTests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Services;
using Entities;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using ApiTests;

public class AuthServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthServiceTest()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            using var scope = _factory.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
        }

        public class RegisterRequest { public string Email { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty; }

        private HttpClient NewClient(bool handleCookies = false) =>
            _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = handleCookies });

        private RegisterRequest RandomCredentials() =>
            new() { Email = $"test{Guid.NewGuid()}@email.com", Password = "SecureP@ssw0rd123!" };

        private async Task<HttpClient> RegisterAndLoginAsync(AuthServiceTest.RegisterRequest creds, bool useCookies)
        {
            var client = NewClient(handleCookies: useCookies);
            await client.PostAsJsonAsync($"/register{(useCookies ? "?useCookies=true" : string.Empty)}", creds);
            await client.PostAsJsonAsync($"/login{(useCookies ? "?useCookies=true" : string.Empty)}", creds);
            return client;
        }

    [Theory]
    [InlineData("/register")]
    public async Task registerNewUser(string url)
    {
        var client = NewClient();
        var req = RandomCredentials();
        var response = await client.PostAsJsonAsync(url, req);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task loginNewUser()
    {
        var creds = RandomCredentials();
        var client = await RegisterAndLoginAsync(creds, useCookies: true);
        // just ensure login completed
    }



    [Fact]
    public async Task testSecureEndpointWithCookie()
    {
        var creds = RandomCredentials();
        var client = await RegisterAndLoginAsync(creds, useCookies: true);
        var afterLoginResponse = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, afterLoginResponse.StatusCode);
    }


    [Fact]
    public async Task testSecureEndpointNoCookie()
    {
        var client = NewClient(handleCookies: true);
        var request = await client.GetAsync("/users/GetAllUsers");
        Assert.Equal(HttpStatusCode.Unauthorized, request.StatusCode);
    }


    [Fact]
    public async Task testLogout()
    {
        var creds = RandomCredentials();
        var client = await RegisterAndLoginAsync(creds, useCookies: true);
        var afterLoginResponse = await client.GetAsync("/");
        afterLoginResponse.EnsureSuccessStatusCode();
        var logoutResponse = await client.PostAsync("/users/logout", null);
        logoutResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SessionEndpointRequiresAuthentication()
    {
        var client = NewClient(handleCookies: true);
        var response = await client.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SessionEndpointReturnsOkWhenAuthenticated()
    {
        var creds = RandomCredentials();
        var client = await RegisterAndLoginAsync(creds, useCookies: true);

        var response = await client.GetAsync("/users/session");

        response.EnsureSuccessStatusCode();
    }


}