using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Tests;

public class RoundServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly GameService _gameService;

    private readonly RoundService _roundService;

    private readonly MatchService _matchService;



    public RoundServiceTest(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        serviceProvider = _serviceProvider;

        scopeFactory = _scopeFactory;

        using (var scope = _serviceProvider.CreateScope())
        {
            _gameService = scope.ServiceProvider.GetRequiredService<GameService>();

            _roundService = scope.ServiceProvider.GetRequiredService<RoundService>();

            _matchService = scope.ServiceProvider.GetRequiredService<MatchService>();
        }
    }

    private async Task<List<Player>> SetupDummyUsersAndPlayers(Guid gameId)
    {

        using (var scope = _scopeFactory.CreateScope())
        {
            List<Player> frontEndPlayers = new List<Player>();

            for (int i = 0; i < 10; i++)
            {
                var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

                var UserProperties = Guid.NewGuid();

                var User = new ApplicationUser
                {
                    Id = UserProperties.ToString(),
                    UserName = UserProperties.ToString(),
                    Email = $"{UserProperties}@mail.com"
                };

                var Player = new Player
                {
                    Id = UserProperties,
                    UserId = UserProperties.ToString(),
                    DisplayName = UserProperties.ToString()
                };

                frontEndPlayers.Add(Player);
                await gs.AddUserToLobby(User, gameId);
            }
            return frontEndPlayers;
        }

    }

    [Fact]
    public async Task StartRoundStartsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        await SetupDummyUsersAndPlayers(gameId);

        await _roundService.StartRound(gameId);
    }

    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var players = await SetupDummyUsersAndPlayers(gameId);

        var currentPlayers = await _roundService.StartRound(gameId);

        var result = await _roundService.EndRoundAsync(gameId, currentPlayers[0], currentPlayers[1]);

        Assert.True(result);
    }
    
    
    

    


}
