
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



    [Theory]
    [InlineData("/register")]
    public async Task registerNewUser(string url)
    {
        var client = _factory.CreateClient();


        var response = await client.PostAsJsonAsync
        (url,
        JsonSerializer.Serialize
            (new
            {
                email = "emailOne@email.com",
                password = "h@rdC0ded"
            })
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Body:\n{errorContent}");
            Console.WriteLine($"---------------------------");
        }

        response.EnsureSuccessStatusCode();


    }
}