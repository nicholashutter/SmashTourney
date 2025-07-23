namespace ApiTests;

using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;
using ApiTests;

public class _gameServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{

    private readonly CustomWebApplicationFactory<Program> _factory;

    private readonly IGameService _gameService;

    public _gameServiceTest()
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

        List<Game> runningGames = await _gameService.GetAllGamesAsync();

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
    public async Task AddPlayersToGameAddsPlayers()
    {
        var gameId = await _gameService.CreateGame();

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        var exception = Record.Exception(() => _gameService.AddPlayersToGame(players, gameId));

    }

    [Fact]
    public async Task GetGameByIdAsyncGetsGameWithMatchingId()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        Assert.Equal(gameId, foundGame.Id);
    }

    private async Task<List<Player>> SetupDummyUsersAndPlayers(Guid gameId)
    {

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            List<Player> players = new List<Player>();

            for (int i = 0; i < 10; i++)
            {
                var _userRepository = scope.ServiceProvider.GetRequiredService<IUserManager>();

                var _playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();

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

                await _playerManager.CreateAsync(Player);

                players.Add(Player);
                _gameService.CreateUserSession(User);
            }
            return players;
        }

    }





    [Fact]
    public async Task UpdateUserScoreUpdatesScoreCorrectly()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        //TODO convert to endMatch testing method;
    }

    [Fact]
    public async Task SaveGame()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0], players[1]);

        _gameService.EndRound(gameId);

        await _gameService.UpdateGameAsync(gameId);

        var exception = Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));
    }

    [Fact]
    public async Task LoadGame()
    {
        var gameId = await _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0], players[1]);

        _gameService.EndRound(gameId);

        await _gameService.UpdateGameAsync(gameId);

        var exception = Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));

    }


    [Fact]
    public async Task StartRoundStartsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        var exception = Record.Exception(() => _gameService.StartRound(gameId));
    }

    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var gameId = await _gameService.CreateGame();

        var players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        var currentPlayers = _gameService.StartRound(gameId);

        var exception = Record.Exception(() => _gameService.EndRound(gameId));
    }


    //need another test to test that game and players are always on same round
    [Fact]
    public async Task StartMatchStartsMatchCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            var players = await SetupDummyUsersAndPlayers(gameId);

            _gameService.AddPlayersToGame(players, gameId);

            _gameService.StartRound(gameId);

            var exception = Record.Exception(() => _gameService.StartMatch(gameId));
        }
    }

    [Fact]
    public async Task EndMatchEndsMatchCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameId = await _gameService.CreateGame();

            var players = await SetupDummyUsersAndPlayers(gameId);

            _gameService.AddPlayersToGame(players, gameId);

            _gameService.StartRound(gameId);

            _gameService.StartMatch(gameId);

            var exception = Record.ExceptionAsync(async () => await _gameService.EndMatchAsync(gameId, players[0], players[1]));
        }
    }

    //TODO need a start to finish test of gameService then we can write route handlers

}