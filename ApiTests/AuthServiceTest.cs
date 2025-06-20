
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;


namespace Tests;

public class AuthServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public struct Request
    {
        public string email;
        public string password;
    }

    public AuthServiceTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }



    [Theory]
    [InlineData("/register")]
    public async Task registerNewUser(string url)
    {
        var client = _factory.CreateClient();


        var response = await client.PostAsJsonAsync(url,
        new Request
        {
            email = "emailOne@email.com",
            password = "h@rdC0ded"
        });

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
       response.Content.Headers.ContentType?.ToString());
    }
}