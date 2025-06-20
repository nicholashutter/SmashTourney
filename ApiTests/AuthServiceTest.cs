
using Microsoft.AspNetCore.Mvc.Testing;
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

    public class Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [Theory]
    [InlineData("/register")]
    public async Task registerNewUser(string url)
    {
        var client = _factory.CreateClient();

        var req = new Request
        {
            Email = "testOne@email.com",
            Password = "SecureP@ssw0rd123!"
        };

        var response = await client.PostAsJsonAsync(url, req);

        response.EnsureSuccessStatusCode();

    }
}