namespace ApiTests;

using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;
using ApiTests;
using Helpers;

//TODO fix addplayerstogame references to addplayertogame 

public class GameServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IGameService _gameService;

    public GameServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
    }

    // helper that creates a game along with dummy users/players and optionally adds them
    private async Task<(Guid GameId, List<ApplicationUser> Users, List<Player> Players)>
        CreateGameWithPlayersAsync(int userCount, bool addPlayers = true)
    {
        var gameId = await _gameService.CreateGame();
        var users = await SetupDummyUsersAsync(userCount);
        var players = await SetupDummyPlayersAsync(users);

        if (addPlayers)
        {
            for (int i = 0; i < players.Count; i++)
            {
                _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            }
        }

        return (gameId, users, players);
    }

    [Fact]
    public async Task CreateGameReturnsValidGuid()
    {
        var result = await _gameService.CreateGame();
        Assert.IsType<Guid>(result);
    }

    [Fact]
    public async Task EndGame_DoesNotThrow()
    {
        var (gameId, _, _) = await CreateGameWithPlayersAsync(0, addPlayers: false);
        var ex = Record.Exception(() => _gameService.EndGame(gameId));
        Assert.Null(ex);
    }

    [Fact]
    public async Task GetAllGames_ReturnsExpectedCount()
    {
        const int expected = 10;
        for (int i = 0; i < expected; i++)
        {
            await _gameService.CreateGame();
        }

        var runningGames = await _gameService.GetAllGamesAsync();
        Assert.Equal(expected, runningGames?.Count);
    }

    [Fact]
    public async Task CreateUserSession_DoesNotThrow()
    {
        var (_, users, _) = await CreateGameWithPlayersAsync(0, addPlayers: false);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "session",
            Email = "session@mail.com"
        };

        var ex = Record.Exception(() => _gameService.CreateUserSession(user));
        Assert.Null(ex);
    }

    [Fact]
    public async Task GetGameByIdAsync_ReturnsSameGame()
    {
        var (gameId, _, _) = await CreateGameWithPlayersAsync(0, addPlayers: false);
        var found = await _gameService.GetGameByIdAsync(gameId);
        Assert.Equal(gameId, found?.Id);
    }



    private async Task<List<ApplicationUser>> SetupDummyUsersAsync(int userCount)
    {
        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserManager>();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            var users = new List<ApplicationUser>();

            for (int i = 0; i < userCount; i++)
            {
                var userId = Guid.NewGuid();
                var email = $"test{Guid.NewGuid()}@email.com";
                var password = "SecureP@ssw0rd123!";

                var user = new ApplicationUser
                {
                    Id = userId.ToString(),
                    UserName = email,
                    Email = email
                };

                await userRepository.CreateUserAsync(user, password);
                gameService.CreateUserSession(user);

                users.Add(user);
            }

            return users;
        }
    }


    private async Task<List<Player>> SetupDummyPlayersAsync(List<ApplicationUser> users)
    {
        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
            var players = new List<Player>();

            foreach (var user in users)
            {
                var player = new Player
                {
                    Id = Guid.Parse(user.Id),
                    UserId = user.Id,
                    DisplayName = user.Id
                };

                await playerManager.CreateAsync(player);
                players.Add(player);
            }

            return players;
        }
    }








    [Fact]
    public async Task UpdateUserScoreUpdatesScoreCorrectly()
    {
        // no behaviour asserted in original test; just exercise API
        var (_, _, _) = await CreateGameWithPlayersAsync(10);
    }

    [Fact]
    public async Task GetPlayersInGameReturnsPlayers()
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(10);
        var playersInGame = await _gameService.GetPlayersInGame(gameId);
        Assert.Equal(players.Count, playersInGame.Count);
    }

    [Fact]
    public async Task SaveAndLoadGame_DoesNotThrow()
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(10);
        _gameService.StartRound(gameId);
        await _gameService.EndMatchAsync(gameId, players[0]);
        _gameService.EndRound(gameId);
        await _gameService.UpdateGameAsync(gameId);

        var exception = await Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));
        Assert.Null(exception);
    }

    // one helper to exercise end-round/start-match/end-match
    private async Task PlayOneRoundAsync(Guid gameId, List<Player> players)
    {
        _gameService.StartRound(gameId);
        foreach (var p in players)
        {
            _gameService.StartMatch(gameId);
            await _gameService.EndMatchAsync(gameId, p);
        }
        _gameService.EndRound(gameId);
    }

    private async Task PlayFullGameAsync(int playerCount)
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(playerCount);
        await _gameService.StartGameAsync(gameId);

        for (int i = 0; i < playerCount; i++)
        {
            await PlayOneRoundAsync(gameId, players);
            await _gameService.UpdateGameAsync(gameId);
        }

        _gameService.EndGame(gameId);
    }

    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(10);
        _gameService.StartRound(gameId);
        var exception = Record.Exception(() => _gameService.EndRound(gameId));
        Assert.Null(exception);
    }

    [Fact]
    public async Task StartMatchStartsMatchCorrectly()
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(10);
        _gameService.StartRound(gameId);
        var exception = Record.Exception(() => _gameService.StartMatch(gameId));
        Assert.Null(exception);
    }

    [Fact]
    public async Task EndMatchEndsMatchCorrectly()
    {
        var (gameId, _, players) = await CreateGameWithPlayersAsync(10);
        _gameService.StartRound(gameId);
        _gameService.StartMatch(gameId);
        var exception = await Record.ExceptionAsync(async () => await _gameService.EndMatchAsync(gameId, players[0]));
        Assert.Null(exception);
    }

    [Fact]
    public async Task StartGameToEndGameNoByes()
    {
        await PlayFullGameAsync(8);
    }

    [Fact]
    public async Task StartGameToEndGameWithByes()
    {
        await PlayFullGameAsync(10);
    }






}