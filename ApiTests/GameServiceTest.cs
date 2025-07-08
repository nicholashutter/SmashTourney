using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Tests;

public class GameServiceTest : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WebApplicationFactory<Program> _factory;

    private readonly MatchService _matchService;

    private readonly RoundService _roundService;

    public GameServiceTest()
    {
        _factory = new WebApplicationFactory<Program>();

        using (var scope = _factory.Services.CreateScope())
        {
            _matchService = scope.ServiceProvider.GetRequiredService<MatchService>();
            _roundService = scope.ServiceProvider.GetRequiredService<RoundService>();
        }
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

    [Fact]

    public async Task EndGameEndsRunningGame()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gs.CreateGame();

            var success = await gs.EndGameAsync(gameId);

            Assert.True(success);
        }

    }

    [Fact]

    public async Task GetAllGamesReturnsAllGames()
    {

        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            Dictionary<int, Guid> inputGames = new Dictionary<int, Guid>();

            //create ten games and store Ids
            for (int i = 0; i < 10; i++)
            {
                var gameId = await gs.CreateGame();
                inputGames.Add(i, gameId);
            }

            List<Game> runningGames = await gs.GetAllGamesAsync();

            Assert.Equal(10, runningGames.Count);
        }

    }

    [Fact]

    public async Task AddUserToLobbyAddsUser()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gs.CreateGame();

            var UserProperties = Guid.NewGuid().ToString();

            var User = new ApplicationUser
            {
                Id = UserProperties,
                UserName = UserProperties,
                Email = $"{UserProperties}@mail.com"
            };

            var success = await gs.AddUserToLobby(User, gameId);

            Assert.True(success);
        }

    }

    [Fact]
    public async Task AddPlayersToGameAddsPlayers()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gs.CreateGame();

            List<Player> frontEndPlayers = new List<Player>();

            for (int i = 0; i < 9; i++)
            {
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

            var success = await gs.AddPlayersToGameAsync(frontEndPlayers, gameId);

            Assert.True(success);
        }

    }

    [Fact]
    public async Task GetGameByIdAsyncGetsGameWithMatchingId()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gs.CreateGame();

            var foundGame = await gs.GetGameByIdAsync(gameId);

            Assert.Equal(gameId, foundGame.Id);
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
    public async Task GenerateBracketAsyncCalculatesByesProperly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gs.CreateGame();

            List<Player> frontEndPlayers = await SetupDummyUsersAndPlayers(gameId);

            await gs.AddPlayersToGameAsync(frontEndPlayers, gameId);

            await gs.GenerateBracketAsync(gameId);

            var foundGame = await gs.GetGameByIdAsync(gameId);

            //number of players determines number of double elimination bracket slots
            //six will always be the bye rounds calculated from 10 players
            Assert.Equal(foundGame.byes, 6);
        }
    }





    [Fact]
    public async Task UpdateUserScoreUpdatesScoreCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gameService.CreateGame();

            var foundGame = await gameService.GetGameByIdAsync(gameId);

            List<Player> frontEndPlayers = await SetupDummyUsersAndPlayers(gameId);

            await gameService.AddPlayersToGameAsync(frontEndPlayers, gameId);

            await gameService.GenerateBracketAsync(gameId);

            await gameService.UpdateUserScore(gameId);
        }
    }

    [Fact]
    public async Task SaveAndLoadGame()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            var gameId = await gameService.CreateGame();

            var foundGame = await gameService.GetGameByIdAsync(gameId);

            List<Player> frontEndPlayers = await SetupDummyUsersAndPlayers(gameId);

            await gameService.AddPlayersToGameAsync(frontEndPlayers, gameId);

            await gameService.GenerateBracketAsync(gameId);

            await gameService.UpdateUserScore(gameId);

            var players = await _roundService.StartRound(gameId);

            await _matchService.EndMatchAsync(gameId, players[0], players[1]);

            await _roundService.EndRoundAsync(gameId, players[0], players[1]);

            await gameService.SaveGameAsync(gameId);

            var success = await gameService.LoadGameAsync(gameId);

            Assert.True(success);
        }
    }


}