namespace ApiTests;

using Entities;
using Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies player repository behavior for player creation and retrieval outcomes.
public class PlayerRepositoryTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    // Initializes test host resources required by player repository tests.
    public PlayerRepositoryTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    // Resolves a player manager from the test host service container.
    private IPlayerManager GetManager()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IPlayerManager>();
    }

    // Confirms that creating a player returns a valid player identifier.
    [Fact]
    public async Task CreateAsyncReturnsValidId()
    {
        var manager = GetManager();
        var result = await manager.CreateAsync(new Player { UserId = AppConstants.ByeUserId });
        Assert.IsType<Guid>(result);
    }

    // Confirms that listing players returns all players created in the test run.
    [Fact]
    public async Task GetAllAsyncReturnsAllPlayers()
    {
        var manager = GetManager();
        const int expected = 10;

        for (var index = 0; index < expected; index++)
        {
            await manager.CreateAsync(new Player { UserId = AppConstants.ByeUserId });
        }

        var result = await manager.GetAllPlayersAsync();
        Assert.Equal(expected, result?.Count);
    }
}