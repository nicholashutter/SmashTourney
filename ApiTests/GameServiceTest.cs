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

    [Fact]
    public async Task CreateGameReturnsValidGUID()
    {
        var result = await _gameService.CreateGame();

        Assert.IsType<Guid>(result);

    }

    [Fact]

    public async Task EndGameEndsRunningGame()
    {
        var gameId = await _gameService.CreateGame();

        var exception = Record.Exception(() => _gameService.EndGame(gameId));

    }

    [Fact]

    public async Task GetAllGamesReturnsAllGames()
    {

        Dictionary<int, Guid> inputGames = new Dictionary<int, Guid>();

        //create ten games and store Ids
        for (int i = 0; i < 10; i++)
        {
            var gameId = await _gameService.CreateGame();
            inputGames.Add(i, gameId);
        }

        List<Game>? runningGames = await _gameService.GetAllGamesAsync();

        Assert.NotNull(runningGames);

        Assert.Equal(10, runningGames.Count);

    }


    [Fact]

    public async Task AddUserToLobbyAddsUser()
    {
        var gameId = await _gameService.CreateGame();

        var UserProperties = Guid.NewGuid().ToString();

        var User = new ApplicationUser
        {
            Id = UserProperties,
            UserName = UserProperties,
            Email = $"{UserProperties}@mail.com"
        };

        var exception = Record.Exception(() => _gameService.CreateUserSession(User));

    }

    [Fact]
    public async Task AddPlayerToGameAddsPlayer()
    {
        var gameId = await _gameService.CreateGame();

        var users = await SetupDummyUsersAsync(10);

        var players = await SetupDummyPlayersAsync(users);

        foreach (var user in users)
        {
            foreach (var player in players)
            {
                var result = _gameService.AddPlayerToGame(player, gameId, user.Id);
                Assert.True(result);
            }
        }

    }

    [Fact]
    public async Task GetGameByIdAsyncGetsGameWithMatchingId()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        Assert.NotNull(foundGame);

        Assert.Equal(gameId, foundGame.Id);
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
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        var users = await SetupDummyUsersAsync(10);
        var players = await SetupDummyPlayersAsync(users);

        for (int i = 0; i < players.Count; i++)
        {
            var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            Assert.True(result);
        }

    }



    [Fact]
    public async Task GetPlayersInGameReturnsPlayers()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        var users = await SetupDummyUsersAsync(10);
        var players = await SetupDummyPlayersAsync(users);

        for (int i = 0; i < players.Count; i++)
        {
            var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            Assert.True(result);
        }

        var playersInGame = await _gameService.GetPlayersInGame(gameId);

        Assert.Equal(players.Count, playersInGame.Count);
    }



    [Fact]
    public async Task SaveGameSavesSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        var users = await SetupDummyUsersAsync(10);
        var players = await SetupDummyPlayersAsync(users);

        for (int i = 0; i < players.Count; i++)
        {
            var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            Assert.True(result);
        }

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0]);

        _gameService.EndRound(gameId);

        await _gameService.UpdateGameAsync(gameId);

        var exception = await Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));
        Assert.Null(exception);
    }



    [Fact]
    public async Task LoadGameLoadsSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        var users = await SetupDummyUsersAsync(10);
        var players = await SetupDummyPlayersAsync(users);

        for (int i = 0; i < players.Count; i++)
        {
            var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            Assert.True(result);
        }

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0]);

        _gameService.EndRound(gameId);

        await _gameService.UpdateGameAsync(gameId);

        var exception = await Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));
        Assert.Null(exception);
    }





    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var users = await SetupDummyUsersAsync(10);
        var players = await SetupDummyPlayersAsync(users);

        for (int i = 0; i < players.Count; i++)
        {
            var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
            Assert.True(result);
        }

        _gameService.StartRound(gameId);

        var exception = Record.Exception(() => _gameService.EndRound(gameId));
        Assert.Null(exception);
    }





    [Fact]
    public async Task StartMatchStartsMatchCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            var users = await SetupDummyUsersAsync(10);
            var players = await SetupDummyPlayersAsync(users);

            for (int i = 0; i < players.Count; i++)
            {
                var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
                Assert.True(result);
            }

            _gameService.StartRound(gameId);

            var exception = Record.Exception(() => _gameService.StartMatch(gameId));
            Assert.Null(exception);
        }
    }



    [Fact]
    public async Task EndMatchEndsMatchCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            var users = await SetupDummyUsersAsync(10);
            var players = await SetupDummyPlayersAsync(users);

            for (int i = 0; i < players.Count; i++)
            {
                var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
                Assert.True(result);
            }

            _gameService.StartRound(gameId);
            _gameService.StartMatch(gameId);

            var exception = await Record.ExceptionAsync(async () => await _gameService.EndMatchAsync(gameId, players[0]));
            Assert.Null(exception);
        }
    }



    [Fact]
    public async Task StartGameToEndGameNoByes()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            const int NUM_PLAYERS_NO_BYES = 8;

            var gameId = await _gameService.CreateGame();

            var users = await SetupDummyUsersAsync(NUM_PLAYERS_NO_BYES);
            var players = await SetupDummyPlayersAsync(users);

            for (int i = 0; i < players.Count; i++)
            {
                var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
                Assert.True(result);
            }

            await _gameService.StartGameAsync(gameId);

            // Test all players play a match for one round, then end the round and game
            for (int i = 0; i < NUM_PLAYERS_NO_BYES; i++)
            {
                _gameService.StartRound(gameId);

                for (int j = 0; j < players.Count; j++)
                {
                    _gameService.StartMatch(gameId);

                    var matchResult = await _gameService.EndMatchAsync(gameId, players[j]);
                    Assert.True(matchResult);
                }

                _gameService.EndRound(gameId);
                await _gameService.UpdateGameAsync(gameId);
            }

            _gameService.EndGame(gameId);
        }
    }



    [Fact]
    public async Task StartGameToEndGameWithByes()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            const int NUM_PLAYERS_TWO_BYES = 10;

            var gameId = await _gameService.CreateGame();

            var users = await SetupDummyUsersAsync(NUM_PLAYERS_TWO_BYES);
            var players = await SetupDummyPlayersAsync(users);

            for (int i = 0; i < players.Count; i++)
            {
                var result = _gameService.AddPlayerToGame(players[i], gameId, users[i].Id);
                Assert.True(result);
            }

            await _gameService.StartGameAsync(gameId);

            // Test all players play a match for one round, then end the round and game
            for (int i = 0; i < NUM_PLAYERS_TWO_BYES; i++)
            {
                _gameService.StartRound(gameId);

                for (int j = 0; j < players.Count; j++)
                {
                    _gameService.StartMatch(gameId);

                    var matchResult = await _gameService.EndMatchAsync(gameId, players[j]);
                    Assert.True(matchResult);
                }

                _gameService.EndRound(gameId);
                await _gameService.UpdateGameAsync(gameId);
            }

            _gameService.EndGame(gameId);
        }
    }




}