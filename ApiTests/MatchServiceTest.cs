using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;
using Entities;

namespace Tests;

public class MatchServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IGameService _gameService;

    private readonly IRoundService _roundService;

    private readonly IMatchService _matchService;

    public MatchServiceTest(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
    {
        serviceProvider = _serviceProvider;
        scopeFactory = _scopeFactory;

        using (var scope = _scopeFactory.CreateScope())
        {
            _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            _roundService = scope.ServiceProvider.GetRequiredService<IRoundService>();

            _matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();
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
    public async Task StartMatchStartsMatchCorrectly()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            await SetupDummyUsersAndPlayers(gameId);

            await _roundService.StartRound(gameId);

            var players = await _matchService.StartMatch(gameId);

            Assert.IsType<List<Player>>(players);

        }
    }

    [Fact]
    public async Task EndMatchEndsMatchCorrectly()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            await SetupDummyUsersAndPlayers(gameId);

            await _roundService.StartRound(gameId);

            var players = await _matchService.StartMatch(gameId);

            var result = await _matchService.EndMatchAsync(gameId, players[0], players[1]);

            Assert.True(result);
        }
    }

    


}