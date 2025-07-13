using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Tests;

public class RoundServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private readonly IGameService _gameService;

    private readonly IRoundService _roundService;

    private readonly IMatchService _matchService;



    public RoundServiceTest()
    {
        _factory = new WebApplicationFactory<Program>();

        using (var scope = _factory.Services.CreateScope())
        {
            _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            _roundService = scope.ServiceProvider.GetRequiredService<IRoundService>();

            _matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();
        }
    }

    private async Task<List<Player>> SetupDummyUsersAndPlayers(Guid gameId)
    {

        using (var scope = _factory.Services.CreateScope())
        {
            List<Player> frontEndPlayers = new List<Player>();

            for (int i = 0; i < 10; i++)
            {
                var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

                var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var UserProperties = Guid.NewGuid();

                var User = new ApplicationUser
                {
                    Id = UserProperties.ToString(),
                    UserName = UserProperties.ToString(),
                    Email = $"{UserProperties}@mail.com"
                };

                await _userRepository.CreateUserAsync(User);

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

        var players = await SetupDummyUsersAndPlayers(gameId);

        await _gameService.AddPlayersToGameAsync(players, gameId);

        await _roundService.StartRound(gameId);
    }

    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var players = await SetupDummyUsersAndPlayers(gameId);

        await _gameService.AddPlayersToGameAsync(players, gameId);

        var currentPlayers = await _roundService.StartRound(gameId);

        var result = await _roundService.EndRoundAsync(gameId, players[0], players[1]);

        Assert.True(result);
    }







}
