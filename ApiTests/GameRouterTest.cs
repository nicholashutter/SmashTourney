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
using System.Net;

public class GameRouterTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    class CreateGameRes { public Guid gameId { get; set; } }
    class GetByIdRes { public Game game { get; set; } }
    class GetAllGamesRes { public List<Game> games { get; set; } }

    public GameRouterTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    public class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }

    private HttpClient NewClient(bool handleCookies = false) =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = handleCookies });

    private async Task<Guid> CreateGameAsync(HttpClient client)
    {
        var resp = await client.PostAsync("/Games/CreateGame", null);
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<CreateGameRes>();
        return data?.gameId ?? Guid.Empty;
    }

    private async Task<List<(HttpClient client, Player player)>> CreateDummyEntitiesWithAuthentication(Guid gameId, int numberOfPlayers)
    {
        var list = new List<(HttpClient, Player)>();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            var client = NewClient(handleCookies: true);
            var email = $"test{i}_{Guid.NewGuid()}@example.com";
            var password = "SecureP@ssw0rd123!";
            var userName = $"Player{i}";

            var registerRequest = new RegisterRequest { Email = email, Password = password };
            (await client.PostAsJsonAsync("/register?useCookies=true", registerRequest)).EnsureSuccessStatusCode();
            (await client.PostAsJsonAsync("/login?useCookies=true", registerRequest)).EnsureSuccessStatusCode();

            list.Add((client, new Player { UserId = "", DisplayName = userName, CurrentCharacter = new Mario(), CurrentGameID = gameId }));
        }
        return list;
    }

    private static object CreateAddPlayerPayload(Guid gameId, string displayName) => new
    {
        id = Guid.NewGuid(),
        displayName,
        currentScore = 0,
        currentRound = 0,
        currentCharacter = new
        {
            id = Guid.NewGuid(),
            characterName = "MARIO",
            archetype = "ALL_ROUNDER",
            fallSpeed = "FAST_FALLERS",
            tierPlacement = "A",
            weightClass = "MIDDLEWEIGHT"
        },
        currentGameID = gameId
    };

    [Fact]
    public async Task CreateGameReturnsSuccess()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        Assert.NotEqual(Guid.Empty, gameId);
    }

    [Fact]
    public async Task CreateUserSessionTakesValidUserReturnsSuccess()
    {
        var client = NewClient();
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "TestUser", Email = Guid.NewGuid() + "@example.com" };
        var response = await client.PostAsJsonAsync("/Games/CreateUserSession", user);
        response.EnsureSuccessStatusCode();
    }





    [Fact]
    public async Task GetAllGamesReturnsAllValidGames()
    {
        var client = NewClient();
        const int expected = 10;
        for (int i = 0; i < expected; i++) await CreateGameAsync(client);
        var getResponse = await client.GetAsync("/Games/getAllGames");
        getResponse.EnsureSuccessStatusCode();
        var responseData = await getResponse.Content.ReadFromJsonAsync<GetAllGamesRes>();
        Assert.Equal(expected, responseData?.games?.Count);
    }


    [Fact]
    public async Task GetGameByIdReturnsValidGame()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        var nextResponse = await client.GetAsync($"/Games/GetGameById/{gameId}");
        nextResponse.EnsureSuccessStatusCode();
        var serializerOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        serializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var nextResponseData = await nextResponse.Content.ReadFromJsonAsync<GetByIdRes>(
            serializerOptions);
        Assert.Equal(gameId, nextResponseData?.game?.Id);
    }

    [Fact]
    public async Task EndGameEndsRunningGame()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        var endResponse = await client.GetAsync($"/Games/EndGame/{gameId}");
        endResponse.EnsureSuccessStatusCode();
    }




    [Fact]
    public async Task AddPlayersReturnsSuccess()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        const int NUM_PLAYERS = 10;
        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);
        foreach (var (playerClient, player) in playerSessions)
        {
            var playerResponse = await playerClient.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", player);
            playerResponse.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task AddPlayerWithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = NewClient();
        var gameId = await CreateGameAsync(unauthenticatedClient);

        var response = await unauthenticatedClient.PostAsJsonAsync(
            $"/Games/AddPlayer/{gameId}",
            CreateAddPlayerPayload(gameId, "NoAuthUser"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SessionStatusWithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = NewClient();

        var response = await unauthenticatedClient.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedCreateTourneyFlow_AllowsSessionAndAddPlayer()
    {
        var gameId = await CreateGameAsync(NewClient());

        var authenticatedClient = NewClient(handleCookies: true);
        var registerRequest = new RegisterRequest
        {
            Email = $"authflow_{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123!"
        };

        (await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest)).EnsureSuccessStatusCode();
        (await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest)).EnsureSuccessStatusCode();

        var sessionResponse = await authenticatedClient.GetAsync("/users/session");
        sessionResponse.EnsureSuccessStatusCode();

        var addPlayerResponse = await authenticatedClient.PostAsJsonAsync(
            $"/Games/AddPlayer/{gameId}",
            CreateAddPlayerPayload(gameId, "AuthFlowUser"));

        addPlayerResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddPlayerWithoutUserIdInBody_UsesClaimUserIdAndReturnsSuccess()
    {
        var gameId = await CreateGameAsync(NewClient());

        var authenticatedClient = NewClient(handleCookies: true);
        var registerRequest = new RegisterRequest
        {
            Email = $"nouserid_{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123!"
        };

        (await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest)).EnsureSuccessStatusCode();
        (await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest)).EnsureSuccessStatusCode();

        var addPlayerPayload = new
        {
            id = Guid.NewGuid(),
            displayName = "NoUserIdBody",
            currentScore = 0,
            currentRound = 0,
            currentCharacter = new
            {
                id = Guid.NewGuid(),
                characterName = 0,
                archetype = 0,
                fallSpeed = 0,
                tierPlacement = 1,
                weightClass = 2
            },
            currentGameID = gameId
        };

        var addPlayerResponse = await authenticatedClient.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", addPlayerPayload);
        addPlayerResponse.EnsureSuccessStatusCode();

        var gameResponse = await authenticatedClient.GetAsync($"/Games/GetGameById/{gameId}");
        gameResponse.EnsureSuccessStatusCode();

        var serializerOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        serializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var gameData = await gameResponse.Content.ReadFromJsonAsync<GetByIdRes>(
            serializerOptions);

        Assert.NotNull(gameData?.game);
        Assert.NotEmpty(gameData!.game.currentPlayers);
        Assert.False(string.IsNullOrWhiteSpace(gameData.game.currentPlayers[0].UserId));
    }

    [Fact]
    public async Task AddPlayerAcceptsStringEnumPayloadFromJoinTourney()
    {
        var gameId = await CreateGameAsync(NewClient());

        var authenticatedClient = NewClient(handleCookies: true);
        var registerRequest = new RegisterRequest
        {
            Email = $"stringenum_{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123!"
        };

        (await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest)).EnsureSuccessStatusCode();
        (await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest)).EnsureSuccessStatusCode();

        var addPlayerPayload = new
        {
            id = Guid.NewGuid(),
            displayName = "StringEnumPlayer",
            currentScore = 0,
            currentRound = 0,
            currentCharacter = new
            {
                id = Guid.NewGuid(),
                characterName = "MARIO",
                archetype = "ALL_ROUNDER",
                fallSpeed = "FAST_FALLERS",
                tierPlacement = "A",
                weightClass = "MIDDLEWEIGHT"
            },
            currentGameID = gameId
        };

        var addPlayerResponse = await authenticatedClient.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", addPlayerPayload);
        addPlayerResponse.EnsureSuccessStatusCode();
    }



    [Fact]
    public async Task StartGameStartsGameSuccessfully()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        const int NUM_PLAYERS = 10;
        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);
        foreach (var (playerClient, player) in playerSessions)
        {
            var playerResponse = await playerClient.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", player);
            playerResponse.EnsureSuccessStatusCode();
        }
        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();
    }




    [Fact]
    public async Task EndGameEndsGameSuccessfully()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        const int NUM_PLAYERS = 10;
        var playerSessions = await CreateDummyEntitiesWithAuthentication(gameId, NUM_PLAYERS);
        foreach (var (playerClient, player) in playerSessions)
        {
            var playerResponse = await playerClient.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", player);
            playerResponse.EnsureSuccessStatusCode();
        }
        var finalResponse = await client.GetAsync($"/Games/EndGame/{gameId}");
        finalResponse.EnsureSuccessStatusCode();
    }



}