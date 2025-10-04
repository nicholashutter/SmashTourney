namespace ApiTests;

using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Services;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Net.Http.Json;

public class GameRouterTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    class CreateGameRes
    {
        public Guid gameId { get; set; }
    }

    class GetByIdRes
    {
        public Game game { get; set; }
    }

    class GetAllGamesRes
    {
        public List<Game> games { get; set; }
    }

    public GameRouterTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }



    private async Task<List<(HttpClient client, Player player)>> CreateDummyEntitiesWithAuthentication(Guid gameId, int numberOfPlayers)
    {
        var result = new List<(HttpClient client, Player player)>();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });

            var email = $"test{i}_{Guid.NewGuid()}@example.com";
            var password = "SecureP@ssw0rd123!";
            var userName = $"Player{i}";

            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password
            };

            var registerResponse = await client.PostAsJsonAsync("/register?useCookies=true", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", registerRequest);
            loginResponse.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();


            var player = new Player
            {
                UserId = "",
                DisplayName = userName,
                CurrentCharacter = new Mario(),
                CurrentGameID = gameId
            };

            result.Add((client, player));
        }

        return result;
    }





    [Fact]
    public async Task CreateGameReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/Games/CreateGame", null);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse);

        Assert.NotNull(responseData);

        var gameId = responseData?.gameId;

        Assert.NotNull(gameId);

        Assert.NotEqual(Guid.Empty, gameId);
    }

    [Fact]
    public async Task CreateUserSessionTakesValidUserReturnsSuccess()
    {
        using var client = _factory.CreateClient();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "TestUser",
            Email = "" + Guid.NewGuid() + "@example.com"
        };

        var userJson = JsonSerializer.Serialize(user);
        var usersContent = new StringContent(userJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/Games/CreateUserSession", usersContent);

        response.EnsureSuccessStatusCode();
    }





    [Fact]
    public async Task GetAllGamesReturnsAllValidGames()
    {
        using var client = _factory.CreateClient();

        for (int i = 0; i < 10; i++)
        {
            var response = await client.PostAsync("/Games/CreateGame", null);

            response.EnsureSuccessStatusCode();

        }

        var getResponse = await client.GetAsync("/Games/getAllGames");

        getResponse.EnsureSuccessStatusCode();


        var jsonResponse = await getResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<GetAllGamesRes>(jsonResponse);

        var games = responseData?.games;

        Assert.NotNull(games);
        Assert.Equal(10, games.Count);
    }


    [Fact]
    public async Task GetGameByIdReturnsValidGame()
    {
        using var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);

        initialResponse.EnsureSuccessStatusCode();

        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseData);

        var gameId = responseData.gameId;

        Assert.NotEqual(Guid.Empty, gameId);

        var nextResponse = await client.GetAsync($"/Games/GetGameById/{gameId}");

        nextResponse.EnsureSuccessStatusCode();

        var nextJsonResponse = await nextResponse.Content.ReadAsStringAsync();

        var nextResponseData = JsonSerializer.Deserialize<GetByIdRes>(nextJsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(nextResponseData.game);
        Assert.Equal(gameId, nextResponseData?.game?.Id);
    }

    [Fact]
    public async Task EndGameEndsRunningGame()
    {
        using var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);

        initialResponse.EnsureSuccessStatusCode();
        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, Guid>>(jsonResponse);
        Assert.NotNull(responseData);
        var gameId = responseData["gameId"];
        Assert.NotEqual(Guid.Empty, gameId);

        var endResponse = await client.GetAsync($"/Games/EndGame/{gameId}");

        endResponse.EnsureSuccessStatusCode();
    }




    [Fact]
    public async Task AddPlayersReturnsSuccess()
    {
        using var scope = _factory.Services.CreateScope();

        var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);
        initialResponse.EnsureSuccessStatusCode();

        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseData);
        var gameId = responseData.gameId;

        const int NUM_PLAYERS = 10;

        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);

        foreach (var (playerClient, player) in playerSessions)
        {
            var playerJson = JsonSerializer.Serialize(player);
            var playerContent = new StringContent(playerJson, System.Text.Encoding.UTF8, "application/json");

            var playerResponse = await playerClient.PostAsync($"/Games/AddPlayer/{gameId}", playerContent);
            playerResponse.EnsureSuccessStatusCode();
        }
    }



    [Fact]
    public async Task StartGameStartsGameSuccessfully()
    {
        using var scope = _factory.Services.CreateScope();

        var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);
        initialResponse.EnsureSuccessStatusCode();

        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseData);
        var gameId = responseData.gameId;

        const int NUM_PLAYERS = 10;

        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);

        foreach (var (playerClient, player) in playerSessions)
        {
            var playerJson = JsonSerializer.Serialize(player);
            var playerContent = new StringContent(playerJson, System.Text.Encoding.UTF8, "application/json");

            var playerResponse = await playerClient.PostAsync($"/Games/AddPlayer/{gameId}", playerContent);
            playerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();
    }




    [Fact]
    public async Task EndGameEndsGameSuccessfully()
    {
        using var scope = _factory.Services.CreateScope();

        var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);
        initialResponse.EnsureSuccessStatusCode();

        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(responseData);
        var gameId = responseData.gameId;

        const int NUM_PLAYERS = 10;

        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);

        foreach (var (playerClient, player) in playerSessions)
        {
            var playerJson = JsonSerializer.Serialize(player);
            var playerContent = new StringContent(playerJson, System.Text.Encoding.UTF8, "application/json");

            var playerResponse = await playerClient.PostAsync($"/Games/AddPlayer/{gameId}", playerContent);
            playerResponse.EnsureSuccessStatusCode();
        }

        var finalResponse = await client.GetAsync($"/Games/EndGame/{gameId}");
        finalResponse.EnsureSuccessStatusCode();
    }



}