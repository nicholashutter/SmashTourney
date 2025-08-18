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

    private Dictionary<Player, ApplicationUser> CreateDummyPlayersAndUsers(Guid gameId, int numberOfPlayers)
    {
        var playerList = new Dictionary<Player, ApplicationUser>();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            var UserId = Guid.NewGuid(); 
            playerList.Add(new Player
            {
                UserId = UserId.ToString(),
                DisplayName = $"Player{i}",
                CurrentCharacter = Enums.CharacterName.MARIO,
                CurrentGameID = gameId
            },
            new ApplicationUser
            {
                Id = UserId.ToString(),
                UserName = $"Player{i}",
                Email = $"" + i + "@example.com"
            });
        }

        return playerList;
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

        const int NUM_PLAYERS = 10; 

        Dictionary<Player, ApplicationUser> playersAndUsers = CreateDummyPlayersAndUsers(gameId, NUM_PLAYERS);

        var playersList = playersAndUsers.Keys.ToList();

        var usersList = playersAndUsers.Values.ToList();
        

        foreach (var user in usersList)

        {
            var userJson = JsonSerializer.Serialize(user);
            var usersContent = new StringContent(userJson, System.Text.Encoding.UTF8, "application/json");
            var userResponse = await client.PostAsync("/Games/CreateUserSession", usersContent);

        userResponse.EnsureSuccessStatusCode();
        }

        var playersJson = JsonSerializer.Serialize(playersList);

        var playersContent = new StringContent(playersJson, System.Text.Encoding.UTF8, "application/json");

        var addResponse = await client.PostAsync($"/Games/AddPlayers/{gameId}", playersContent);

        addResponse.EnsureSuccessStatusCode();

    }
    
  [Fact]
  public async Task StartGameStartsGameSuccessfully()
  {
      using var client = _factory.CreateClient();

      const int NUM_PLAYERS = 10;

      var initialResponse = await client.PostAsync("/Games/CreateGame", null);

      initialResponse.EnsureSuccessStatusCode();

      var jsonResponse = await initialResponse.Content.ReadAsStringAsync();
      var responseData = JsonSerializer.Deserialize<CreateGameRes>(jsonResponse, new JsonSerializerOptions
      {
          PropertyNameCaseInsensitive = true
      });

      Assert.NotNull(responseData);
      var gameId = responseData.gameId;

      var playersAndUsers = CreateDummyPlayersAndUsers(gameId, NUM_PLAYERS);

        var playersList = playersAndUsers.Keys.ToList();

        var usersList = playersAndUsers.Values.ToList();

      var playersJson = JsonSerializer.Serialize(playersList);

      var playersContent = new StringContent(playersJson, System.Text.Encoding.UTF8, "application/json");

        

        foreach (var user in usersList)
        {
            var usesrJson = JsonSerializer.Serialize(user);

            var usersContent = new StringContent(usesrJson, System.Text.Encoding.UTF8, "application/json");
            var secondResponse = await client.PostAsync("/Games/CreateUserSession", usersContent);

            secondResponse.EnsureSuccessStatusCode();
        }

        var thirdResponse = await client.PostAsync($"/Games/AddPlayers/{gameId}", playersContent);

    thirdResponse.EnsureSuccessStatusCode();

      var fourthResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);

      fourthResponse.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task EndGameEndsGameSuccessfully()
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

        const int NUM_PLAYERS = 10;

        var playersAndUsers = CreateDummyPlayersAndUsers(gameId, NUM_PLAYERS);

        var playersList = playersAndUsers.Keys.ToList();
        var usersList = playersAndUsers.Values.ToList();


      var playersJson = JsonSerializer.Serialize(playersList);

      var playersContent = new StringContent(playersJson, System.Text.Encoding.UTF8, "application/json");

      foreach (var user in usersList)
        {
            var usesrJson = JsonSerializer.Serialize(user);

            var usersContent = new StringContent(usesrJson, System.Text.Encoding.UTF8, "application/json");
            var innerResponse = await client.PostAsync("/Games/CreateUserSession", usersContent);
            innerResponse.EnsureSuccessStatusCode();
        }

        

      var secondResponse = await client.PostAsync($"/Games/AddPlayers/{gameId}", playersContent);

      secondResponse.EnsureSuccessStatusCode();

      var finalResponse = await client.GetAsync($"/Games/EndGame/{gameId}");

      finalResponse.EnsureSuccessStatusCode();
  }
      






}