namespace ApiTests;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Services;

public class GameRouterTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public GameRouterTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateGameReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/Games/CreateGame", null);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, Guid>>(jsonResponse);

        Assert.NotNull(responseData);

        var gameId = responseData["gameId"];

        Assert.NotEqual(Guid.Empty, gameId);
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
        var games = JsonSerializer.Deserialize<List<Game>>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

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
        var responseData = JsonSerializer.Deserialize<Dictionary<string, Guid>>(jsonResponse);

        Assert.NotNull(responseData);

        var gameId = responseData["gameId"];

        Assert.NotEqual(Guid.Empty, gameId);

        var nextResponse = await client.GetAsync($"/Games/GetGameById/{gameId}");

        nextResponse.EnsureSuccessStatusCode();

        var nextJsonResponse = await nextResponse.Content.ReadAsStringAsync();

        var nextResponseData = JsonSerializer.Deserialize<Dictionary<string, Game>>(jsonResponse);

        Assert.NotNull(nextResponseData);
        Assert.Equal(gameId, nextResponseData["game"].Id);
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

    private List<Player> CreateDummyPlayers(Guid gameId)
    {
        var playerList = new List<Player>();

        for (int i = 0; i < 10; i++)
        {
            playerList.Add(new Player
            {
                UserId = Guid.NewGuid().ToString(),
                DisplayName = $"Player{i}",
                CurrentCharacter = Enums.CharacterName.MARIO,
                CurrentGameID = gameId,
                CurrentOpponent = Guid.NewGuid()
            });
        }

        return playerList;
    }

    [Fact]
    public async Task AddPlayersReturnsSuccess()
    {
        using var client = _factory.CreateClient();
        var initialResponse = await client.PostAsync("/Games/CreateGame", null);
        initialResponse.EnsureSuccessStatusCode();
        var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, Guid>>(jsonResponse);
        Assert.NotNull(responseData);
        var gameId = responseData["gameId"];
        Assert.NotEqual(Guid.Empty, gameId);

        var playersList = CreateDummyPlayers(gameId);

        var playersJson = JsonSerializer.Serialize(playersList);

        var playersContent = new StringContent(playersJson, System.Text.Encoding.UTF8, "application/json");

        var addResponse = await client.PostAsync($"/Games/AddPlayers/{gameId}", playersContent);

        addResponse.EnsureSuccessStatusCode();

    }
        





}