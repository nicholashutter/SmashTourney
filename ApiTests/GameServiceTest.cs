using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Tests;

public class _gameServiceTest : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WebApplicationFactory<Program> _factory;

    private readonly IGameService _gameService;

    public _gameServiceTest()
    {
        _factory = new WebApplicationFactory<Program>();

        using (var scope = _factory.Services.CreateScope())
        {
            _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        }
    }

    [Fact]
    public void CreateGameReturnsValidGUID()
    {
        var result = _gameService.CreateGame();

        Assert.IsType<Guid>(result);

    }

    [Fact]

    public void EndGameEndsRunningGame()
    {
        var gameId = _gameService.CreateGame();

        var exception = Record.Exception(() => _gameService.EndGame(gameId));

    }

    [Fact]

    public async Task GetAllGamesReturnsAllGames()
    {

        Dictionary<int, Guid> inputGames = new Dictionary<int, Guid>();

        //create ten games and store Ids
        for (int i = 0; i < 10; i++)
        {
            var gameId = _gameService.CreateGame();
            inputGames.Add(i, gameId);
        }

        List<Game> runningGames = await _gameService.GetAllGamesAsync();

        Assert.Equal(10, runningGames.Count);

    }


    [Fact]

    public void AddUserToLobbyAddsUser()
    {
        var gameId = _gameService.CreateGame();

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
    public void AddPlayersToGameAddsPlayers()
    {
        var gameId = _gameService.CreateGame();

        List<Player> players = new List<Player>();

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

            players.Add(Player);
            _gameService.CreateUserSession(User);
        }

        var exception = Record.Exception(() => _gameService.AddPlayersToGame(players, gameId));

    }

    [Fact]
    public async Task GetGameByIdAsyncGetsGameWithMatchingId()
    {
        var gameId = _gameService.CreateGame();

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
    public async Task GenerateBracketAsyncCalculatesByesProperly()
    {
        var gameId = _gameService.CreateGame();

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.GenerateBracket(gameId);

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        //number of players determines number of double elimination bracket slots
        //six will always be the bye rounds calculated from 10 players
        Assert.Equal(foundGame.byes, 6);
    }





    [Fact]
    public async Task UpdateUserScoreUpdatesScoreCorrectly()
    {
        var gameId = _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.GenerateBracket(gameId);

        var exception = Record.ExceptionAsync(async () => await _gameService.UpdateUserScoreAsync(gameId));
    }

    [Fact]
    public async Task SaveGame()
    {
        var gameId = _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.GenerateBracket(gameId);

        await _gameService.UpdateUserScoreAsync(gameId);

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0], players[1]);

        _gameService.EndRound(gameId);

        await _gameService.SaveGameAsync(gameId);

        var exception = Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));
    }

    [Fact]
    public async Task LoadGame()
    {
        var gameId = _gameService.CreateGame();

        var foundGame = await _gameService.GetGameByIdAsync(gameId);

        List<Player> players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        _gameService.GenerateBracket(gameId);

        await _gameService.UpdateUserScoreAsync(gameId);

        _gameService.StartRound(gameId);

        await _gameService.EndMatchAsync(gameId, players[0], players[1]);

        _gameService.EndRound(gameId);

        await _gameService.SaveGameAsync(gameId);

        var exception = Record.ExceptionAsync(async () => await _gameService.LoadGameAsync(gameId));

    }


    [Fact]
    public async Task StartRoundStartsRoundSuccessfully()
    {
        var gameId = _gameService.CreateGame();

        var players = await SetupDummyUsersAndPlayers(gameId);

        _gameService.AddPlayersToGame(players, gameId);

        var exception = Record.Exception(() => _gameService.StartRound(gameId));
    }

    [Fact]
    public async Task EndRoundAsyncEndsRoundSuccessfully()
    {
        var gameId = _gameService.CreateGame();

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
            var gameId = _gameService.CreateGame();

            var players = await SetupDummyUsersAndPlayers(gameId);

            _gameService.AddPlayersToGame(players, gameId);

            _gameService.StartRound(gameId);

            var exception = Record.Exception(() => _gameService.StartMatch(gameId));
        }
    }

    //need another test to test that game and players are always on same round
    [Fact]
    public async Task EndMatchEndsMatchCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var gameId = _gameService.CreateGame();

            var players = await SetupDummyUsersAndPlayers(gameId);

            _gameService.AddPlayersToGame(players, gameId);

            _gameService.StartRound(gameId);

            _gameService.StartMatch(gameId);

            var exception = Record.ExceptionAsync(async () => await _gameService.EndMatchAsync(gameId, players[0], players[1]));
        }
    }

}