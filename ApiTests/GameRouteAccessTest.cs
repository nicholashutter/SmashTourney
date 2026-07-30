namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTests.TestContracts;
using Contracts;
using Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies over HTTP that a signed-in stranger is refused a game they are not in.
//
// The service-level checks are covered elsewhere; these exist because the guards
// live in the routes, and a check that is written but not wired to an endpoint
// protects nothing. Every caller here is genuinely authenticated with a real
// account and a real cookie — the previous gate — and is still turned away.
public class GameRouteAccessTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public GameRouteAccessTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Creates a signed-in client with its own freshly registered, confirmed account.
    //
    // Confirmation is part of the flow now, so this goes through the shared helper
    // that registers, reads the emailed link, follows it and signs in.
    private async Task<HttpClient> CreateAuthenticatedClientAsync(string emailPrefix)
    {
        var account = await AuthenticatedClientFactory.CreateAsync(_factory, emailPrefix);
        return account.Client;
    }

    // Creates a game owned by the given client.
    private async Task<Guid> CreateGameAsync(HttpClient hostClient)
    {
        var response = await hostClient.PostAsJsonAsync(
            "/Games/CreateGameWithMode",
            new { bracketMode = BracketMode.SINGLE_ELIMINATION.ToString() });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateGameWithModeResponse>(SerializerOptions);
        Assert.NotNull(payload);

        return payload!.GameId;
    }

    // Joins a client to a game as a player.
    private async Task JoinGameAsync(HttpClient client, Guid gameId, string displayName)
    {
        var playerPayload = new
        {
            id = Guid.NewGuid(),
            displayName,
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

        var response = await client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", playerPayload);
        response.EnsureSuccessStatusCode();
    }

    // Confirms every game read is refused to a signed-in stranger.
    //
    // 404 rather than 403 throughout: a 403 would confirm the game exists, which
    // is the one fact somebody walking game ids is trying to collect.
    [Theory]
    [InlineData("GET", "/Games/GetBracket")]
    [InlineData("GET", "/Games/GetCurrentMatch")]
    [InlineData("GET", "/Games/GetFlowState")]
    [InlineData("GET", "/Games/GetPlayerSession")]
    public async Task ReadingAnotherPersonsGameIsNotFound(string httpMethod, string routePrefix)
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var outsiderClient = await CreateAuthenticatedClientAsync("outsider");

        var request = new HttpRequestMessage(new HttpMethod(httpMethod), $"{routePrefix}/{gameId}");
        var response = await outsiderClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Confirms the roster of another person's game is not readable.
    [Fact]
    public async Task ReadingAnotherPersonsRosterIsNotFound()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var outsiderClient = await CreateAuthenticatedClientAsync("outsider");

        var response = await outsiderClient.PostAsJsonAsync($"/Games/GetPlayersInGame/{gameId}", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Confirms a stranger cannot start somebody else's tournament.
    //
    // This route had no check at all beyond being signed in, so any account that
    // knew a game id could lock a stranger's lobby and seed their bracket around
    // whoever had joined at that moment.
    [Fact]
    public async Task AStrangerCannotStartAnotherPersonsTournament()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var outsiderClient = await CreateAuthenticatedClientAsync("outsider");

        var response = await outsiderClient.PostAsync($"/Games/StartGame/{gameId}", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // And the game really is still waiting, not merely reported as refused.
        var flowResponse = await hostClient.GetAsync($"/Games/GetFlowState/{gameId}");
        flowResponse.EnsureSuccessStatusCode();

        var flowState = await flowResponse.Content.ReadFromJsonAsync<GameStateResponse>(SerializerOptions);
        Assert.False(flowState!.GameStarted);
    }

    // Confirms a joined player who is not the host cannot start the tournament.
    //
    // A participant is told plainly that this is the host's call, because unlike a
    // stranger they already know the game exists.
    [Fact]
    public async Task AJoinedPlayerWhoIsNotHostIsForbiddenFromStarting()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var playerClient = await CreateAuthenticatedClientAsync("player");
        await JoinGameAsync(playerClient, gameId, "Player Two");

        var response = await playerClient.PostAsync($"/Games/StartGame/{gameId}", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Confirms the host is still able to start their own tournament.
    //
    // Guards that lock out the legitimate caller are the usual way this goes
    // wrong, so the positive case is asserted alongside the refusals.
    [Fact]
    public async Task TheHostCanStillStartTheirOwnTournament()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var playerClient = await CreateAuthenticatedClientAsync("player");
        await JoinGameAsync(playerClient, gameId, "Player Two");

        var response = await hostClient.PostAsync($"/Games/StartGame/{gameId}", null);

        Assert.True(response.IsSuccessStatusCode);
    }

    // Confirms a joined player can still read the game they are in.
    [Fact]
    public async Task AJoinedPlayerCanStillReadTheirOwnGame()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var playerClient = await CreateAuthenticatedClientAsync("player");
        await JoinGameAsync(playerClient, gameId, "Player Two");

        (await playerClient.GetAsync($"/Games/GetFlowState/{gameId}")).EnsureSuccessStatusCode();
        (await playerClient.PostAsJsonAsync($"/Games/GetPlayersInGame/{gameId}", new { })).EnsureSuccessStatusCode();
        (await playerClient.GetAsync($"/Games/GetPlayerSession/{gameId}")).EnsureSuccessStatusCode();
    }

    // Confirms the listing does not disclose other people's tournaments.
    [Fact]
    public async Task TheListingRouteDoesNotDiscloseOtherPeoplesTournaments()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var hiddenGameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, hiddenGameId, "Host");

        var outsiderClient = await CreateAuthenticatedClientAsync("outsider");

        var response = await outsiderClient.GetAsync("/Games/GetActiveGames");
        response.EnsureSuccessStatusCode();

        var summaries = await response.Content.ReadFromJsonAsync<List<GameSummaryResponse>>(SerializerOptions);

        Assert.NotNull(summaries);
        Assert.DoesNotContain(summaries!, summary => summary.GameId == hiddenGameId);
    }

    // Confirms the listing still shows a caller their own tournament.
    [Fact]
    public async Task TheListingRouteShowsTheCallersOwnTournament()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Host");

        var response = await hostClient.GetAsync("/Games/GetActiveGames");
        response.EnsureSuccessStatusCode();

        var summaries = await response.Content.ReadFromJsonAsync<List<GameSummaryResponse>>(SerializerOptions);

        Assert.Contains(summaries!, summary => summary.GameId == gameId);
    }

    // Confirms the players route no longer hands over the whole table.
    //
    // Any signed-in account could read every player row in the database, which on
    // a public host is the display name of everyone who has ever played.
    [Fact]
    public async Task ThePlayersRouteReturnsOnlyTheCallersOwnPlayers()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "SomebodyElse");

        var outsiderClient = await CreateAuthenticatedClientAsync("outsider");

        var response = await outsiderClient.GetAsync("/Players");
        response.EnsureSuccessStatusCode();

        var players = await response.Content.ReadFromJsonAsync<List<PlayerAccessView>>(SerializerOptions);

        Assert.NotNull(players);
        Assert.DoesNotContain(players!, player => player.DisplayName == "SomebodyElse");
    }

    // Confirms one account cannot delete another account's player.
    //
    // This was an unscoped delete behind nothing but a login, so a single
    // registered account could have removed every player in every tournament.
    [Fact]
    public async Task OneAccountCannotDeleteAnotherAccountsPlayer()
    {
        var hostClient = await CreateAuthenticatedClientAsync("host");
        var gameId = await CreateGameAsync(hostClient);
        await JoinGameAsync(hostClient, gameId, "Victim");

        var rosterResponse = await hostClient.PostAsJsonAsync($"/Games/GetPlayersInGame/{gameId}", new { });
        rosterResponse.EnsureSuccessStatusCode();

        using var rosterJson = JsonDocument.Parse(await rosterResponse.Content.ReadAsStringAsync());
        var victimId = rosterJson.RootElement
            .GetProperty("currentPlayers")
            .EnumerateArray()
            .First()
            .GetProperty("id")
            .GetString();

        var attackerClient = await CreateAuthenticatedClientAsync("attacker");

        var deleteResponse = await attackerClient.DeleteAsync($"/Players/{victimId}");

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        // The victim is still on the roster afterwards.
        var afterResponse = await hostClient.PostAsJsonAsync($"/Games/GetPlayersInGame/{gameId}", new { });
        afterResponse.EnsureSuccessStatusCode();

        using var afterJson = JsonDocument.Parse(await afterResponse.Content.ReadAsStringAsync());
        Assert.NotEmpty(afterJson.RootElement.GetProperty("currentPlayers").EnumerateArray());
    }
}

// The subset of a player record these tests read back from the players route.
public sealed class PlayerAccessView
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
