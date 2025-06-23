using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Tests;

public class GameServiceTest:IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WebApplicationFactory<Program> _factory;

    public GameServiceTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateGameReturnsValidGUID()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var result = await gs.CreateGame();

            Assert.IsType<Guid>(result);
        }

    }
    
}