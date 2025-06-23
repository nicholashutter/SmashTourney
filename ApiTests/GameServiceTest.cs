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

    public GameServiceTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public void ClearDB()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

    }

    [Fact]
    public async Task CreateGameReturnsValidGUID()
    {
        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

                var result = await gs.CreateGame();

                Assert.IsType<Guid>(result);
            }
        }
        finally
        {
            ClearDB();
        }

    }

    [Fact]

    public async Task EndGameEndsRunningGame()
    {
        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var gs = scope.ServiceProvider.GetRequiredService<IGameService>();

                var gameId = await gs.CreateGame();

                var success = await gs.EndGameAsync(gameId);

                Assert.True(success);
            }
        }
        finally
        {
            ClearDB();
        }

    }

    [Fact]

    public async Task GetAllGamesReturnsAllGames()
    {

        try
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
        finally
        {
            ClearDB();
        }

    }

    [Fact]

    public async Task AddUserToLobbyAddsUser()
    {
        try
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
        finally
        {
            ClearDB();
        }

    }

    [Fact]
    public async Task AddPlayersToGameAddsPlayers()
    {
        try
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
        finally
        {
            ClearDB();
        }

    }

}