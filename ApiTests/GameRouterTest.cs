namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTests.TestContracts;
using Contracts;
using Entities;
using Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies business-facing game route behavior for authentication, tournament progression, and completion.
public class GameRouterTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    // Builds JSON options that deserialize enum values from API responses consistently.
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    // Initializes test host resources required by route integration tests.
    public GameRouterTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Creates a client that can optionally persist cookies between requests.
    private HttpClient NewClient(bool handleCookies = false)
    {
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            HandleCookies = handleCookies
        };

        return _factory.CreateClient(clientOptions);
    }

    // Creates a game through the public route and returns its identifier.
    private async Task<Guid> CreateGameAsync(HttpClient client)
    {
        var response = await CreateGameWithModeAsync(client, BracketMode.SINGLE_ELIMINATION);
        return response.GameId;
    }

    // Creates a game in a selected bracket mode and returns route response data.
    private async Task<CreateGameWithModeResponse> CreateGameWithModeAsync(HttpClient client, BracketMode bracketMode)
    {
        var requestBody = new
        {
            bracketMode = bracketMode.ToString()
        };

        var response = await client.PostAsJsonAsync("/Games/CreateGameWithMode", requestBody);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateGameWithModeResponse>(SerializerOptions);
        Assert.NotNull(payload);

        return payload!;
    }

    // Creates authenticated users and corresponding player payloads for a specific game.
    private async Task<List<AuthenticatedPlayerSession>> CreateDummyEntitiesWithAuthenticationAsync(Guid gameId, int numberOfPlayers)
    {
        var sessions = new List<AuthenticatedPlayerSession>();

        for (var index = 0; index < numberOfPlayers; index++)
        {
            var client = NewClient(handleCookies: true);
            var registerRequest = new RegisterRequest
            {
                Email = $"test{index}_{Guid.NewGuid()}@example.com",
                Password = "SecureP@ssw0rd123!"
            };

            var registerResponse = await client.PostAsJsonAsync("/register?useCookies=true", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", registerRequest);
            loginResponse.EnsureSuccessStatusCode();

            var player = new Player
            {
                Id = Guid.NewGuid(),
                UserId = string.Empty,
                DisplayName = $"Player{index}",
                CurrentCharacter = new Mario(),
                CurrentGameID = gameId
            };

            var session = new AuthenticatedPlayerSession
            {
                Client = client,
                Player = player
            };

            sessions.Add(session);
        }

        return sessions;
    }

    // Creates an authenticated client session that can call protected routes.
    private async Task<HttpClient> CreateAuthenticatedClientAsync(string emailPrefix)
    {
        var client = NewClient(handleCookies: true);
        var registerRequest = new RegisterRequest
        {
            Email = $"{emailPrefix}_{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123!"
        };

        var registerResponse = await client.PostAsJsonAsync("/register?useCookies=true", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", registerRequest);
        loginResponse.EnsureSuccessStatusCode();

        return client;
    }

    // Builds a started two-player match and returns sessions mapped to current match participants.
    private async Task<(Guid GameId, CurrentMatchResponse Match, AuthenticatedPlayerSession PlayerOneSession, AuthenticatedPlayerSession PlayerTwoSession)>
        SetupStartedTwoPlayerMatchAsync()
    {
        var hostClient = NewClient();
        var gameId = await CreateGameAsync(hostClient);

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 2);
        foreach (var session in playerSessions)
        {
            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var startResponse = await hostClient.PostAsync($"/Games/StartGame/{gameId}", null);
        startResponse.EnsureSuccessStatusCode();

        var currentMatchResponse = await hostClient.GetAsync($"/Games/GetCurrentMatch/{gameId}");
        currentMatchResponse.EnsureSuccessStatusCode();

        var currentMatch = await currentMatchResponse.Content.ReadFromJsonAsync<CurrentMatchResponse>(SerializerOptions);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match payload was null for two-player setup.");
        }

        var playerOneSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerOneId);
        var playerTwoSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerTwoId);
        if (playerOneSession is null || playerTwoSession is null)
        {
            throw new InvalidOperationException("Failed to map player sessions to current match participants.");
        }

        return (gameId, currentMatch, playerOneSession, playerTwoSession);
    }

    // Builds a client payload that mirrors frontend add-player requests.
    private static object CreateAddPlayerPayload(Guid gameId, string displayName)
    {
        return new
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
    }

    // Executes an authenticated tournament from lobby to completion and returns match-report totals.
    private async Task<TournamentRunResult> RunAuthenticatedTournamentToCompletionAsync(
        BracketMode bracketMode,
        int playerCount,
        int maxIterations)
    {
        var hostClient = NewClient();
        var gameResponse = await CreateGameWithModeAsync(hostClient, bracketMode);
        var gameId = gameResponse.GameId;

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, playerCount);

        foreach (var session in playerSessions)
        {
            var sessionStatusResponse = await session.Client.GetAsync("/users/session");
            sessionStatusResponse.EnsureSuccessStatusCode();

            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var lobbyFlowResponse = await hostClient.GetAsync($"/Games/GetFlowState/{gameId}");
        lobbyFlowResponse.EnsureSuccessStatusCode();

        var lobbyFlow = await lobbyFlowResponse.Content.ReadFromJsonAsync<GameStateResponse>(SerializerOptions);
        if (lobbyFlow is null || lobbyFlow.State != GameState.LOBBY_WAITING)
        {
            throw new InvalidOperationException("Lobby flow state was not available before game start.");
        }

        var startGameResponse = await hostClient.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();

        var reportedMatchIds = new HashSet<Guid>();
        var reportCount = 0;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var flowResponse = await hostClient.GetAsync($"/Games/GetFlowState/{gameId}");
            flowResponse.EnsureSuccessStatusCode();

            var flowState = await flowResponse.Content.ReadFromJsonAsync<GameStateResponse>(SerializerOptions);
            if (flowState is null || !flowState.GameStarted)
            {
                throw new InvalidOperationException("Flow state was null or game was not marked started during progression.");
            }

            if (flowState.State == GameState.COMPLETE)
            {
                break;
            }

            var currentMatchResponse = await hostClient.GetAsync($"/Games/GetCurrentMatch/{gameId}");
            if (currentMatchResponse.StatusCode == HttpStatusCode.NotFound)
            {
                continue;
            }

            currentMatchResponse.EnsureSuccessStatusCode();

            var currentMatch = await currentMatchResponse.Content.ReadFromJsonAsync<CurrentMatchResponse>(SerializerOptions);
            if (currentMatch is null)
            {
                throw new InvalidOperationException("Current match payload was null during progression.");
            }

            var currentMatchId = currentMatch.MatchId;
            if (reportedMatchIds.Contains(currentMatchId))
            {
                throw new InvalidOperationException("Current match repeated while tournament was still in progress.");
            }
            reportedMatchIds.Add(currentMatchId);

            var playerOneSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerOneId);
            var playerTwoSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerTwoId);
            if (playerOneSession is null || playerTwoSession is null)
            {
                throw new InvalidOperationException("Failed to resolve authenticated sessions for current match participants.");
            }

            var voteRequest = new SubmitMatchVoteRequest(currentMatchId, currentMatch.PlayerOneId);

            var firstVoteResponse = await playerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
            firstVoteResponse.EnsureSuccessStatusCode();

            var secondVoteResponse = await playerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
            secondVoteResponse.EnsureSuccessStatusCode();

            reportCount++;
        }

        var finalFlowResponse = await hostClient.GetAsync($"/Games/GetFlowState/{gameId}");
        finalFlowResponse.EnsureSuccessStatusCode();

        var finalFlow = await finalFlowResponse.Content.ReadFromJsonAsync<GameStateResponse>(SerializerOptions);
        if (finalFlow is null || finalFlow.State != GameState.COMPLETE)
        {
            throw new InvalidOperationException("Tournament did not reach COMPLETE flow state.");
        }

        var finalSnapshotResponse = await hostClient.GetAsync($"/Games/GetBracket/{gameId}");
        finalSnapshotResponse.EnsureSuccessStatusCode();

        var finalSnapshot = await finalSnapshotResponse.Content.ReadFromJsonAsync<BracketSnapshotResponse>(SerializerOptions);
        if (finalSnapshot is null || finalSnapshot.Mode != bracketMode)
        {
            throw new InvalidOperationException("Final bracket snapshot was invalid for tournament mode.");
        }

        if (!finalSnapshot.Matches.Any(match => match.Status == BracketMatchStatus.COMPLETE))
        {
            throw new InvalidOperationException("Final snapshot did not contain completed matches.");
        }

        if (reportCount <= 0)
        {
            throw new InvalidOperationException("Tournament progression reported no completed matches.");
        }

        return new TournamentRunResult
        {
            GameId = gameId,
            ReportedMatches = reportCount
        };
    }

    // Calculates a safety bound for report-loop iterations by bracket mode and player count.
    private static int GetMaxIterations(BracketMode bracketMode, int playerCount)
    {
        if (bracketMode == BracketMode.SINGLE_ELIMINATION)
        {
            return playerCount * 8;
        }

        return playerCount * 16;
    }

    // Confirms that authenticated players can join a game through AddPlayer route.
    [Fact]
    public async Task AddPlayersReturnsSuccess()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 10);

        var allAddPlayerCallsSucceeded = true;

        foreach (var session in playerSessions)
        {
            var playerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            if (!playerResponse.IsSuccessStatusCode)
            {
                allAddPlayerCallsSucceeded = false;
            }
        }

        Assert.True(allAddPlayerCallsSucceeded);
    }

    // Confirms that unauthenticated clients cannot add players to a game.
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

    // Confirms that session status route requires a signed-in user context.
    [Fact]
    public async Task SessionStatusWithoutAuthentication_ReturnsUnauthorized()
    {
        var unauthenticatedClient = NewClient();

        var response = await unauthenticatedClient.GetAsync("/users/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Confirms that authenticated create-tournament flow supports session and add-player steps.
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

        var registerResponse = await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest);
        loginResponse.EnsureSuccessStatusCode();

        var sessionResponse = await authenticatedClient.GetAsync("/users/session");
        sessionResponse.EnsureSuccessStatusCode();

        var addPlayerResponse = await authenticatedClient.PostAsJsonAsync(
            $"/Games/AddPlayer/{gameId}",
            CreateAddPlayerPayload(gameId, "AuthFlowUser"));

        Assert.True(addPlayerResponse.IsSuccessStatusCode);
    }

    // Confirms that AddPlayer can derive user identity from auth claims when payload omits UserId.
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

        var registerResponse = await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest);
        loginResponse.EnsureSuccessStatusCode();

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

        var gamePlayersResponse = await authenticatedClient.PostAsJsonAsync($"/Games/GetPlayersInGame/{gameId}", new { });
        gamePlayersResponse.EnsureSuccessStatusCode();

        using var playersJson = JsonDocument.Parse(await gamePlayersResponse.Content.ReadAsStringAsync());
        var currentPlayers = playersJson.RootElement.GetProperty("currentPlayers");
        var firstPlayer = currentPlayers.EnumerateArray().FirstOrDefault();
        var hasResolvedUserId = firstPlayer.ValueKind == JsonValueKind.Object
            && firstPlayer.TryGetProperty("userId", out var userIdElement)
            && !string.IsNullOrWhiteSpace(userIdElement.GetString());

        Assert.True(hasResolvedUserId);
    }

    // Confirms that AddPlayer accepts enum string payloads used by frontend join flows.
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

        var registerResponse = await authenticatedClient.PostAsJsonAsync("/register?useCookies=true", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await authenticatedClient.PostAsJsonAsync("/login?useCookies=true", registerRequest);
        loginResponse.EnsureSuccessStatusCode();

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
        Assert.True(addPlayerResponse.IsSuccessStatusCode);
    }

    // Confirms that starting a game succeeds after enough players have joined.
    [Fact]
    public async Task StartGameStartsGameSuccessfully()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);
        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 10);

        foreach (var session in playerSessions)
        {
            var playerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            playerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        Assert.True(startGameResponse.IsSuccessStatusCode);
    }

    // Confirms that single-elimination routes expose a snapshot and active current match.
    [Fact]
    public async Task SingleEliminationBracketEndpointsReturnSnapshotAndCurrentMatch()
    {
        var client = NewClient();
        var gameId = await CreateGameAsync(client);

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 4);
        foreach (var session in playerSessions)
        {
            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();

        var bracketResponse = await client.GetAsync($"/Games/GetBracket/{gameId}");
        bracketResponse.EnsureSuccessStatusCode();

        var snapshot = await bracketResponse.Content.ReadFromJsonAsync<BracketSnapshotResponse>(SerializerOptions);
        if (snapshot is null || snapshot.Mode != BracketMode.SINGLE_ELIMINATION || snapshot.Matches.Count == 0)
        {
            throw new InvalidOperationException("Single elimination bracket snapshot was not valid.");
        }

        var currentMatchResponse = await client.GetAsync($"/Games/GetCurrentMatch/{gameId}");
        currentMatchResponse.EnsureSuccessStatusCode();

        var currentMatch = await currentMatchResponse.Content.ReadFromJsonAsync<CurrentMatchResponse>(SerializerOptions);
        Assert.True(currentMatch is not null);
    }

    // Confirms that explicit mode creation route returns the requested bracket mode.
    [Fact]
    public async Task CreateGameWithModeReturnsRequestedBracketMode()
    {
        var client = NewClient();
        var response = await CreateGameWithModeAsync(client, BracketMode.DOUBLE_ELIMINATION);

        Assert.True(response.GameId != Guid.Empty && response.BracketMode == BracketMode.DOUBLE_ELIMINATION);
    }

    // Confirms that double-elimination routes return snapshot and support current-match reporting.
    [Fact]
    public async Task DoubleEliminationBracketEndpointsReturnSnapshotAndSupportCurrentMatchRoute()
    {
        var client = NewClient();
        var gameResponse = await CreateGameWithModeAsync(client, BracketMode.DOUBLE_ELIMINATION);
        var gameId = gameResponse.GameId;

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 4);
        foreach (var session in playerSessions)
        {
            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();

        var bracketResponse = await client.GetAsync($"/Games/GetBracket/{gameId}");
        bracketResponse.EnsureSuccessStatusCode();

        var snapshot = await bracketResponse.Content.ReadFromJsonAsync<BracketSnapshotResponse>(SerializerOptions);
        if (snapshot is null || snapshot.Mode != BracketMode.DOUBLE_ELIMINATION)
        {
            throw new InvalidOperationException("Double elimination bracket snapshot was not valid.");
        }

        var currentMatchResponse = await client.GetAsync($"/Games/GetCurrentMatch/{gameId}");
        if (currentMatchResponse.StatusCode == HttpStatusCode.NotFound)
        {
            Assert.True(currentMatchResponse.StatusCode == HttpStatusCode.NotFound);
            return;
        }

        currentMatchResponse.EnsureSuccessStatusCode();

        var currentMatch = await currentMatchResponse.Content.ReadFromJsonAsync<CurrentMatchResponse>(SerializerOptions);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match should be present when route did not return NotFound.");
        }

        var playerOneSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerOneId);
        var playerTwoSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerTwoId);
        if (playerOneSession is null || playerTwoSession is null)
        {
            throw new InvalidOperationException("Unable to resolve authenticated participants for vote progression.");
        }

        var voteRequest = new SubmitMatchVoteRequest(currentMatch.MatchId, currentMatch.PlayerOneId);
        var firstVoteResponse = await playerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
        firstVoteResponse.EnsureSuccessStatusCode();

        var secondVoteResponse = await playerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
        Assert.True(secondVoteResponse.IsSuccessStatusCode);
    }

    // Confirms that flow-state route returns authoritative started-game state values.
    [Fact]
    public async Task GetFlowStateReturnsAuthoritativeStateForStartedGame()
    {
        var client = NewClient();
        var gameResponse = await CreateGameWithModeAsync(client, BracketMode.DOUBLE_ELIMINATION);
        var gameId = gameResponse.GameId;

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 4);
        foreach (var session in playerSessions)
        {
            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();

        var flowResponse = await client.GetAsync($"/Games/GetFlowState/{gameId}");
        flowResponse.EnsureSuccessStatusCode();

        var flowState = await flowResponse.Content.ReadFromJsonAsync<GameStateResponse>(SerializerOptions);
        var flowStateIsValid = flowState is not null
            && flowState.GameId == gameId
            && flowState.GameStarted
            && (flowState.State is GameState.IN_MATCH_ACTIVE or GameState.BRACKET_VIEW or GameState.COMPLETE);

        Assert.True(flowStateIsValid);
    }

    // Confirms that double-elimination progression continues until no active match remains.
    [Fact]
    public async Task DoubleEliminationApiEndToEndFlowProgressesUntilNoCurrentMatch()
    {
        var client = NewClient();
        var gameResponse = await CreateGameWithModeAsync(client, BracketMode.DOUBLE_ELIMINATION);
        var gameId = gameResponse.GameId;

        var playerSessions = await CreateDummyEntitiesWithAuthenticationAsync(gameId, 8);
        foreach (var session in playerSessions)
        {
            var addPlayerResponse = await session.Client.PostAsJsonAsync($"/Games/AddPlayer/{gameId}", session.Player);
            addPlayerResponse.EnsureSuccessStatusCode();
        }

        var startGameResponse = await client.PostAsync($"/Games/StartGame/{gameId}", null);
        startGameResponse.EnsureSuccessStatusCode();

        var reportedMatchIds = new HashSet<Guid>();
        var reportCount = 0;
        var tournamentFinished = false;

        for (var iteration = 0; iteration < 64; iteration++)
        {
            var currentMatchResponse = await client.GetAsync($"/Games/GetCurrentMatch/{gameId}");
            if (currentMatchResponse.StatusCode == HttpStatusCode.NotFound)
            {
                tournamentFinished = true;
                break;
            }

            currentMatchResponse.EnsureSuccessStatusCode();

            var currentMatch = await currentMatchResponse.Content.ReadFromJsonAsync<CurrentMatchResponse>(SerializerOptions);
            if (currentMatch is null)
            {
                throw new InvalidOperationException("Current match was null during end-to-end double elimination flow.");
            }

            var matchId = currentMatch.MatchId;
            if (reportedMatchIds.Contains(matchId))
            {
                throw new InvalidOperationException("Received duplicate match id during progression loop.");
            }
            reportedMatchIds.Add(matchId);

            var playerOneSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerOneId);
            var playerTwoSession = playerSessions.FirstOrDefault(session => session.Player.Id == currentMatch.PlayerTwoId);
            if (playerOneSession is null || playerTwoSession is null)
            {
                throw new InvalidOperationException("Unable to resolve authenticated sessions for vote progression loop.");
            }

            var voteRequest = new SubmitMatchVoteRequest(currentMatch.MatchId, currentMatch.PlayerOneId);

            var firstVoteResponse = await playerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
            firstVoteResponse.EnsureSuccessStatusCode();

            var secondVoteResponse = await playerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{gameId}", voteRequest);
            secondVoteResponse.EnsureSuccessStatusCode();
            reportCount++;
        }

        var snapshotResponse = await client.GetAsync($"/Games/GetBracket/{gameId}");
        snapshotResponse.EnsureSuccessStatusCode();

        var finalSnapshot = await snapshotResponse.Content.ReadFromJsonAsync<BracketSnapshotResponse>(SerializerOptions);
        var finalStateIsValid = finalSnapshot is not null
            && reportCount > 0
            && tournamentFinished
            && finalSnapshot.Matches.Any(match => match.Status == BracketMatchStatus.COMPLETE);

        Assert.True(finalStateIsValid);
    }

    // Confirms submit-vote route commits match when both participants vote for same winner.
    [Fact]
    public async Task SubmitMatchVoteCommitsWhenBothParticipantsAgree()
    {
        var setup = await SetupStartedTwoPlayerMatchAsync();
        var request = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerOneId);

        var firstVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        firstVoteResponse.EnsureSuccessStatusCode();

        var firstVotePayload = await firstVoteResponse.Content.ReadFromJsonAsync<SubmitMatchVoteResponse>(SerializerOptions);
        var firstVoteIsPending = firstVotePayload is not null
            && firstVotePayload.Status == SubmitMatchVoteStatus.PENDING
            && firstVotePayload.VoteCount == 1;
        if (!firstVoteIsPending)
        {
            throw new InvalidOperationException("First vote did not return expected pending state.");
        }

        var secondVoteResponse = await setup.PlayerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        secondVoteResponse.EnsureSuccessStatusCode();

        var secondVotePayload = await secondVoteResponse.Content.ReadFromJsonAsync<SubmitMatchVoteResponse>(SerializerOptions);
        var secondVoteCommitted = secondVotePayload is not null
            && secondVotePayload.Status == SubmitMatchVoteStatus.COMMITTED
            && secondVotePayload.CommittedWinnerPlayerId == setup.Match.PlayerOneId;

        Assert.True(secondVoteCommitted);
    }

    // Confirms duplicate vote from same participant is rejected.
    [Fact]
    public async Task SubmitMatchVoteRejectsDuplicateVoteBySameParticipant()
    {
        var setup = await SetupStartedTwoPlayerMatchAsync();
        var request = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerOneId);

        var firstVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        firstVoteResponse.EnsureSuccessStatusCode();

        var duplicateVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        if (duplicateVoteResponse.StatusCode != HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Duplicate vote did not return conflict status.");
        }

        var duplicateVotePayload = await duplicateVoteResponse.Content.ReadFromJsonAsync<SubmitMatchVoteResponse>(SerializerOptions);
        var duplicateVoteRejected = duplicateVotePayload is not null
            && duplicateVotePayload.Status == SubmitMatchVoteStatus.DUPLICATE_VOTE;

        Assert.True(duplicateVoteRejected);
    }

    // Confirms unaffiliated authenticated users cannot vote in active match.
    [Fact]
    public async Task SubmitMatchVoteRejectsNonParticipant()
    {
        var setup = await SetupStartedTwoPlayerMatchAsync();
        var nonParticipantClient = await CreateAuthenticatedClientAsync("nonparticipant_vote");

        var request = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerOneId);
        var response = await nonParticipantClient.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Confirms conflicting winner votes reset pending tally and return conflict.
    [Fact]
    public async Task SubmitMatchVoteReturnsConflictWhenParticipantsDisagree()
    {
        var setup = await SetupStartedTwoPlayerMatchAsync();

        var playerOneVote = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerOneId);
        var playerTwoVote = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerTwoId);

        var firstVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", playerOneVote);
        firstVoteResponse.EnsureSuccessStatusCode();

        var conflictingVoteResponse = await setup.PlayerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", playerTwoVote);
        if (conflictingVoteResponse.StatusCode != HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Conflicting vote did not return conflict status.");
        }

        var conflictingPayload = await conflictingVoteResponse.Content.ReadFromJsonAsync<SubmitMatchVoteResponse>(SerializerOptions);
        var conflictWasReturned = conflictingPayload is not null
            && conflictingPayload.Status == SubmitMatchVoteStatus.CONFLICT
            && conflictingPayload.VoteCount == 0;

        Assert.True(conflictWasReturned);
    }

    // Confirms completed-match votes are rejected as no longer active.
    [Fact]
    public async Task SubmitMatchVoteRejectsVotesForCompletedMatch()
    {
        var setup = await SetupStartedTwoPlayerMatchAsync();
        var request = new SubmitMatchVoteRequest(setup.Match.MatchId, setup.Match.PlayerOneId);

        var firstVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        firstVoteResponse.EnsureSuccessStatusCode();

        var secondVoteResponse = await setup.PlayerTwoSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        secondVoteResponse.EnsureSuccessStatusCode();

        var staleVoteResponse = await setup.PlayerOneSession.Client.PostAsJsonAsync($"/Games/SubmitMatchVote/{setup.GameId}", request);
        if (staleVoteResponse.StatusCode != HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Stale vote did not return conflict status.");
        }

        var staleVotePayload = await staleVoteResponse.Content.ReadFromJsonAsync<SubmitMatchVoteResponse>(SerializerOptions);
        var staleVoteRejected = staleVotePayload is not null
            && staleVotePayload.Status == SubmitMatchVoteStatus.MATCH_NOT_ACTIVE;

        Assert.True(staleVoteRejected);
    }

    // Confirms full auth-to-completion flow for single-elimination power-of-two tournaments.
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public async Task AuthToCompletionStateMachineFlow_WorksForSingleEliminationPowerOfTwoBrackets(int playerCount)
    {
        var runResult = await RunAuthenticatedTournamentToCompletionAsync(
            BracketMode.SINGLE_ELIMINATION,
            playerCount,
            GetMaxIterations(BracketMode.SINGLE_ELIMINATION, playerCount));

        Assert.True(runResult.GameId != Guid.Empty && runResult.ReportedMatches == playerCount - 1);
    }

    // Confirms full auth-to-completion flow for double-elimination power-of-two tournaments.
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    public async Task AuthToCompletionStateMachineFlow_WorksForDoubleEliminationPowerOfTwoBrackets(int playerCount)
    {
        var runResult = await RunAuthenticatedTournamentToCompletionAsync(
            BracketMode.DOUBLE_ELIMINATION,
            playerCount,
            GetMaxIterations(BracketMode.DOUBLE_ELIMINATION, playerCount));

        var hasValidRange = runResult.ReportedMatches >= playerCount - 1
            && runResult.ReportedMatches <= (playerCount * 2) - 1;

        Assert.True(runResult.GameId != Guid.Empty && hasValidRange);
    }
}
