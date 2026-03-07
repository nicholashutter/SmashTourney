namespace ApiTests;

using ApiTests.TestContracts;
using Contracts;
using Entities;
using Enums;
using Helpers;
using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies service-layer tournament behavior from game setup through bracket progression.
public class GameServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IGameService _gameService;

    // Initializes service test dependencies and ensures database availability.
    public GameServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();

        _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
    }

    // Creates a game plus users and players and optionally adds players to the game.
    private async Task<GameSetupResult> CreateGameWithPlayersAsync(int userCount, bool addPlayers = true)
    {
        var gameId = await _gameService.CreateGame();
        var users = await SetupDummyUsersAsync(userCount);
        var players = await SetupDummyPlayersAsync(users);

        if (addPlayers)
        {
            for (var index = 0; index < players.Count; index++)
            {
                _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
            }
        }

        return new GameSetupResult
        {
            GameId = gameId,
            Users = users,
            Players = players
        };
    }

    // Creates test users and corresponding in-memory sessions.
    private async Task<List<ApplicationUser>> SetupDummyUsersAsync(int userCount)
    {
        using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        var users = new List<ApplicationUser>();

        for (var index = 0; index < userCount; index++)
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

            await userManager.CreateUserAsync(user, password);
            gameService.CreateUserSession(user);
            users.Add(user);
        }

        return users;
    }

    // Creates player entities for each user in the test run.
    private async Task<List<Player>> SetupDummyPlayersAsync(List<ApplicationUser> users)
    {
        using var scope = _factory.Services.CreateAsyncScope();
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

    // Confirms game creation returns a valid identifier.
    [Fact]
    public async Task CreateGameReturnsValidGuid()
    {
        var result = await _gameService.CreateGame();
        Assert.IsType<Guid>(result);
    }

    // Confirms ending a game does not throw for valid game state.
    [Fact]
    public async Task EndGameDoesNotThrow()
    {
        var gameSetup = await CreateGameWithPlayersAsync(0, addPlayers: false);

        var exception = Record.Exception(() => _gameService.EndGame(gameSetup.GameId));

        Assert.Null(exception);
    }

    // Confirms game listing returns expected game count.
    [Fact]
    public async Task GetAllGamesReturnsExpectedCount()
    {
        var baselineGames = await _gameService.GetAllGamesAsync();
        var baselineCount = baselineGames?.Count ?? 0;
        const int expectedCount = 10;

        for (var index = 0; index < expectedCount; index++)
        {
            await _gameService.CreateGame();
        }

        var runningGames = await _gameService.GetAllGamesAsync();
        Assert.Equal(baselineCount + expectedCount, runningGames?.Count ?? 0);
    }

    // Confirms creating a user session succeeds for valid user payload.
    [Fact]
    public void CreateUserSessionDoesNotThrow()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "session",
            Email = "session@mail.com"
        };

        var exception = Record.Exception(() => _gameService.CreateUserSession(user));
        Assert.Null(exception);
    }

    // Confirms retrieving a game by ID returns the same game record.
    [Fact]
    public async Task GetGameByIdAsyncReturnsSameGame()
    {
        var gameSetup = await CreateGameWithPlayersAsync(0, addPlayers: false);
        var found = await _gameService.GetGameByIdAsync(gameSetup.GameId);

        Assert.Equal(gameSetup.GameId, found?.Id);
    }

    // Confirms players can be retrieved for a game after assignment.
    [Fact]
    public async Task GetPlayersInGameReturnsPlayers()
    {
        var gameSetup = await CreateGameWithPlayersAsync(10);

        var playersInGame = await _gameService.GetPlayersInGame(gameSetup.GameId);

        Assert.Equal(gameSetup.Players.Count, playersInGame.Count);
    }

    // Confirms double-elimination game start produces a bracket snapshot with ready matches.
    [Fact]
    public async Task DoubleEliminationStartGameInitializesBracketSnapshot()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.DOUBLE_ELIMINATION));
        var users = await SetupDummyUsersAsync(4);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for valid double-elimination setup.");
        }

        var snapshot = await _gameService.GetBracketSnapshotAsync(gameId);
        var snapshotIsValid = snapshot is not null
            && snapshot.Mode == BracketMode.DOUBLE_ELIMINATION
            && snapshot.Matches.Count > 0
            && snapshot.Matches.Any(match => match.Status == BracketMatchStatus.READY);

        Assert.True(snapshotIsValid);
    }

    // Confirms reported double-elimination match state persists and reloads correctly.
    [Fact]
    public async Task DoubleEliminationReportMatchPersistsAndHydratesAcrossLoadGame()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.DOUBLE_ELIMINATION));
        var users = await SetupDummyUsersAsync(4);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed before persistence verification.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match was not available after game start.");
        }

        var reportSuccess = await _gameService.ReportMatchResultAsync(
            gameId,
            new ReportMatchRequest(currentMatch.MatchId, currentMatch.PlayerOneId));
        if (!reportSuccess)
        {
            throw new InvalidOperationException("ReportMatchResultAsync returned false for a valid match.");
        }

        var beforeUnloadSnapshot = await _gameService.GetBracketSnapshotAsync(gameId);
        if (beforeUnloadSnapshot is null)
        {
            throw new InvalidOperationException("Bracket snapshot before unload was null.");
        }

        var completedBeforeUnload = beforeUnloadSnapshot.Matches.Count(match => match.Status == BracketMatchStatus.COMPLETE);
        if (completedBeforeUnload < 1)
        {
            throw new InvalidOperationException("Expected at least one completed match before reload.");
        }

        await _gameService.UpdateGameAsync(gameId);

        using var newScope = _factory.Services.CreateScope();
        var rehydratedService = newScope.ServiceProvider.GetRequiredService<IGameService>();

        var loaded = await rehydratedService.LoadGameAsync(gameId);
        if (!loaded)
        {
            throw new InvalidOperationException("LoadGameAsync failed for a persisted game.");
        }

        var afterLoadSnapshot = await rehydratedService.GetBracketSnapshotAsync(gameId);
        var afterLoadSnapshotIsValid = afterLoadSnapshot is not null
            && afterLoadSnapshot.Mode == BracketMode.DOUBLE_ELIMINATION
            && afterLoadSnapshot.Matches.Count(match => match.Status == BracketMatchStatus.COMPLETE) == completedBeforeUnload;

        Assert.True(afterLoadSnapshotIsValid);
    }

    // Confirms started games return one of the valid started flow states.
    [Fact]
    public async Task GetGameStateAsyncReturnsStartedStateAfterStartGame()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION));
        var users = await SetupDummyUsersAsync(4);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var startResult = await _gameService.StartGameAsync(gameId);
        if (!startResult)
        {
            throw new InvalidOperationException("StartGameAsync failed for a valid single elimination setup.");
        }

        var gameState = await _gameService.GetGameStateAsync(gameId);
        var gameStateIsValid = gameState is not null
            && gameState.GameStarted
            && (gameState.State is GameState.IN_MATCH_ACTIVE or GameState.BRACKET_VIEW or GameState.COMPLETE);

        Assert.True(gameStateIsValid);
    }

    // Confirms service vote ledger commits result when both participants vote for same winner.
    [Fact]
    public async Task SubmitMatchVoteAsyncCommitsWhenTwoParticipantsAgree()
    {
        var gameSetup = await CreateGameWithPlayersAsync(2);

        var startResult = await _gameService.StartGameAsync(gameSetup.GameId);
        if (!startResult)
        {
            throw new InvalidOperationException("StartGameAsync failed for vote-ledger service test.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameSetup.GameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match was null after StartGameAsync.");
        }

        var playerOneUserId = gameSetup.Players.First(player => player.Id == currentMatch.PlayerOneId).UserId;
        var playerTwoUserId = gameSetup.Players.First(player => player.Id == currentMatch.PlayerTwoId).UserId;

        var voteRequest = new SubmitMatchVoteRequest(currentMatch.MatchId, currentMatch.PlayerOneId);

        var firstVote = await _gameService.SubmitMatchVoteAsync(gameSetup.GameId, playerOneUserId, voteRequest);
        if (firstVote.Status != SubmitMatchVoteStatus.PENDING)
        {
            throw new InvalidOperationException("First vote did not produce pending status.");
        }

        var secondVote = await _gameService.SubmitMatchVoteAsync(gameSetup.GameId, playerTwoUserId, voteRequest);

        var committed = secondVote.Status == SubmitMatchVoteStatus.COMMITTED
            && secondVote.CommittedWinnerPlayerId == currentMatch.PlayerOneId;

        Assert.True(committed);
    }

    // Confirms service vote ledger rejects duplicate vote from same voter on a pending match.
    [Fact]
    public async Task SubmitMatchVoteAsyncRejectsDuplicateVoteFromSameVoter()
    {
        var gameSetup = await CreateGameWithPlayersAsync(2);

        var startResult = await _gameService.StartGameAsync(gameSetup.GameId);
        if (!startResult)
        {
            throw new InvalidOperationException("StartGameAsync failed for duplicate vote service test.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameSetup.GameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match was null for duplicate vote service test.");
        }

        var voterUserId = gameSetup.Players.First(player => player.Id == currentMatch.PlayerOneId).UserId;
        var voteRequest = new SubmitMatchVoteRequest(currentMatch.MatchId, currentMatch.PlayerOneId);

        var firstVote = await _gameService.SubmitMatchVoteAsync(gameSetup.GameId, voterUserId, voteRequest);
        if (firstVote.Status != SubmitMatchVoteStatus.PENDING)
        {
            throw new InvalidOperationException("First vote did not produce pending status in duplicate test.");
        }

        var duplicateVote = await _gameService.SubmitMatchVoteAsync(gameSetup.GameId, voterUserId, voteRequest);

        Assert.True(duplicateVote.Status == SubmitMatchVoteStatus.DUPLICATE_VOTE);
    }

    // Confirms bye auto-resolution advances odd-sized brackets to a real-vs-real active match.
    [Fact]
    public async Task StartGameAsyncWithOddPlayersAutoResolvesByesBeforeFirstRealMatch()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 8));
        var users = await SetupDummyUsersAsync(5);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for odd-player bye progression test.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match should exist after bye auto-resolution for odd-player setup.");
        }

        var realPlayerIds = players.Select(player => player.Id).ToHashSet();
        var isRealVsRealMatch = realPlayerIds.Contains(currentMatch.PlayerOneId)
            && realPlayerIds.Contains(currentMatch.PlayerTwoId);

        Assert.True(isRealVsRealMatch);
    }

    // Confirms one real player against one bye is auto-completed without requiring any vote.
    [Fact]
    public async Task StartGameAsyncAutoCompletesRealVersusByeWithoutManualVote()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 2));
        var users = await SetupDummyUsersAsync(1);
        var players = await SetupDummyPlayersAsync(users);

        _gameService.AddPlayerToGame(players[0], gameId, users[0].Id);

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for real-vs-bye auto-complete test.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        var flowState = await _gameService.GetGameStateAsync(gameId);

        var autoCompleted = currentMatch is null
            && flowState is not null
            && flowState.State == GameState.COMPLETE;

        Assert.True(autoCompleted);
    }

    // Confirms bye-vs-bye matches are auto-resolved and do not stall bracket progression.
    [Fact]
    public async Task StartGameAsyncAutoCompletesByeVersusByeWithoutManualVote()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 2));

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for bye-vs-bye auto-complete test.");
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        var flowState = await _gameService.GetGameStateAsync(gameId);
        var snapshot = await _gameService.GetBracketSnapshotAsync(gameId);

        var hasCompletedBracket = snapshot is not null
            && snapshot.Matches.Any(match => match.Status == BracketMatchStatus.COMPLETE);

        var progressionDidNotStall = currentMatch is null
            && flowState is not null
            && flowState.State == GameState.COMPLETE
            && hasCompletedBracket;

        Assert.True(progressionDidNotStall);
    }

    // Confirms progression advances past bye rounds when bye metadata is missing in runtime state.
    [Fact]
    public async Task SubmitMatchVoteAsyncAdvancesPastByeWhenByeMetadataIsMissing()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 4));
        var users = await SetupDummyUsersAsync(3);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for missing-bye-metadata regression test.");
        }

        var gameServiceConcrete = _gameService as GameService;
        if (gameServiceConcrete is null)
        {
            throw new InvalidOperationException("Unable to access concrete GameService for regression test setup.");
        }

        var bracketStatesField = typeof(GameService).GetField("_bracketStates", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bracketStatesField?.GetValue(gameServiceConcrete) is not IDictionary bracketStates)
        {
            throw new InvalidOperationException("Unable to access bracket runtime state dictionary for regression test setup.");
        }

        if (!bracketStates.Contains(gameId))
        {
            throw new InvalidOperationException("Expected in-memory bracket runtime state for regression test setup.");
        }

        var runtimeState = bracketStates[gameId];
        if (runtimeState is null)
        {
            throw new InvalidOperationException("Runtime bracket state instance was null in regression test setup.");
        }

        var byePlayerIdsProperty = runtimeState.GetType().GetProperty("ByePlayerIds", BindingFlags.Instance | BindingFlags.Public);
        var byePlayerIds = byePlayerIdsProperty?.GetValue(runtimeState);
        var clearMethod = byePlayerIds?.GetType().GetMethod("Clear", Type.EmptyTypes);
        if (clearMethod is null)
        {
            throw new InvalidOperationException("Unable to clear bye metadata in runtime state for regression test setup.");
        }

        clearMethod.Invoke(byePlayerIds, null);

        var firstMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (firstMatch is null)
        {
            throw new InvalidOperationException("Current match was null before first vote in regression test.");
        }

        var firstVoteWinnerId = firstMatch.PlayerOneId;
        var firstMatchPlayerOneUserId = players.First(player => player.Id == firstMatch.PlayerOneId).UserId;
        var firstMatchPlayerTwoUserId = players.First(player => player.Id == firstMatch.PlayerTwoId).UserId;

        var firstMatchVoteRequest = new SubmitMatchVoteRequest(firstMatch.MatchId, firstVoteWinnerId);
        var firstVote = await _gameService.SubmitMatchVoteAsync(gameId, firstMatchPlayerOneUserId, firstMatchVoteRequest);
        if (firstVote.Status != SubmitMatchVoteStatus.PENDING)
        {
            throw new InvalidOperationException("First vote should be pending for first real-vs-real match in regression test.");
        }

        var secondVote = await _gameService.SubmitMatchVoteAsync(gameId, firstMatchPlayerTwoUserId, firstMatchVoteRequest);
        if (secondVote.Status != SubmitMatchVoteStatus.COMMITTED)
        {
            throw new InvalidOperationException("Second vote should commit first real-vs-real match in regression test.");
        }

        var secondMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (secondMatch is null)
        {
            throw new InvalidOperationException("Expected second match after first match commit in regression test.");
        }

        var realPlayerIds = players.Select(player => player.Id).ToHashSet();
        var isRealVsRealMatch = realPlayerIds.Contains(secondMatch.PlayerOneId)
            && realPlayerIds.Contains(secondMatch.PlayerTwoId);

        Assert.True(isRealVsRealMatch);
    }

    // Confirms current-match resolution still skips bye matches when bye metadata is missing.
    [Fact]
    public async Task GetCurrentMatchAsyncSkipsByeMatchesWhenByeMetadataIsMissing()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 4));
        var users = await SetupDummyUsersAsync(3);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for missing-bye-metadata current-match regression test.");
        }

        var gameServiceConcrete = _gameService as GameService;
        if (gameServiceConcrete is null)
        {
            throw new InvalidOperationException("Unable to access concrete GameService for current-match regression setup.");
        }

        var bracketStatesField = typeof(GameService).GetField("_bracketStates", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bracketStatesField?.GetValue(gameServiceConcrete) is not IDictionary bracketStates)
        {
            throw new InvalidOperationException("Unable to access bracket runtime state dictionary for current-match regression setup.");
        }

        if (!bracketStates.Contains(gameId))
        {
            throw new InvalidOperationException("Expected in-memory bracket runtime state for current-match regression setup.");
        }

        var runtimeState = bracketStates[gameId];
        if (runtimeState is null)
        {
            throw new InvalidOperationException("Runtime bracket state instance was null in current-match regression setup.");
        }

        var byePlayerIdsProperty = runtimeState.GetType().GetProperty("ByePlayerIds", BindingFlags.Instance | BindingFlags.Public);
        var byePlayerIds = byePlayerIdsProperty?.GetValue(runtimeState);
        var clearMethod = byePlayerIds?.GetType().GetMethod("Clear", Type.EmptyTypes);
        if (clearMethod is null)
        {
            throw new InvalidOperationException("Unable to clear bye metadata in runtime state for current-match regression setup.");
        }

        clearMethod.Invoke(byePlayerIds, null);

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match should exist after bye auto-resolution in current-match regression test.");
        }

        var realPlayerIds = players.Select(player => player.Id).ToHashSet();
        var isRealVsRealMatch = realPlayerIds.Contains(currentMatch.PlayerOneId)
            && realPlayerIds.Contains(currentMatch.PlayerTwoId);

        Assert.True(isRealVsRealMatch);
    }

    // Confirms current-match resolution skips bye rounds in double-elimination when bye metadata is missing.
    [Fact]
    public async Task GetCurrentMatchAsyncSkipsByeMatchesWhenByeMetadataIsMissingForDoubleElimination()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.DOUBLE_ELIMINATION, TotalPlayers: 4));
        var users = await SetupDummyUsersAsync(3);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for double-elimination missing-bye-metadata regression test.");
        }

        var firstMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (firstMatch is null)
        {
            throw new InvalidOperationException("Current match should exist before metadata mutation in double-elimination regression test.");
        }

        var gameServiceConcrete = _gameService as GameService;
        if (gameServiceConcrete is null)
        {
            throw new InvalidOperationException("Unable to access concrete GameService for double-elimination regression setup.");
        }

        var bracketStatesField = typeof(GameService).GetField("_bracketStates", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bracketStatesField?.GetValue(gameServiceConcrete) is not IDictionary bracketStates)
        {
            throw new InvalidOperationException("Unable to access bracket runtime state dictionary for double-elimination regression setup.");
        }

        if (!bracketStates.Contains(gameId))
        {
            throw new InvalidOperationException("Expected in-memory bracket runtime state for double-elimination regression setup.");
        }

        var runtimeState = bracketStates[gameId];
        if (runtimeState is null)
        {
            throw new InvalidOperationException("Runtime bracket state instance was null in double-elimination regression setup.");
        }

        var byePlayerIdsProperty = runtimeState.GetType().GetProperty("ByePlayerIds", BindingFlags.Instance | BindingFlags.Public);
        var byePlayerIds = byePlayerIdsProperty?.GetValue(runtimeState);
        var clearMethod = byePlayerIds?.GetType().GetMethod("Clear", Type.EmptyTypes);
        if (clearMethod is null)
        {
            throw new InvalidOperationException("Unable to clear bye metadata in runtime state for double-elimination regression setup.");
        }

        clearMethod.Invoke(byePlayerIds, null);

        var firstMatchWinnerUserId = players.First(player => player.Id == firstMatch.PlayerOneId).UserId;
        var firstMatchLoserUserId = players.First(player => player.Id == firstMatch.PlayerTwoId).UserId;
        var firstMatchVoteRequest = new SubmitMatchVoteRequest(firstMatch.MatchId, firstMatch.PlayerOneId);

        var firstVote = await _gameService.SubmitMatchVoteAsync(gameId, firstMatchWinnerUserId, firstMatchVoteRequest);
        if (firstVote.Status != SubmitMatchVoteStatus.PENDING)
        {
            throw new InvalidOperationException("First vote should be pending for first real-vs-real match in double-elimination regression test.");
        }

        var secondVote = await _gameService.SubmitMatchVoteAsync(gameId, firstMatchLoserUserId, firstMatchVoteRequest);
        if (secondVote.Status != SubmitMatchVoteStatus.COMMITTED)
        {
            throw new InvalidOperationException("Second vote should commit first real-vs-real match in double-elimination regression test.");
        }

        var nextMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (nextMatch is null)
        {
            throw new InvalidOperationException("Expected next match after first match commit in double-elimination regression test.");
        }

        var realPlayerIds = players.Select(player => player.Id).ToHashSet();
        var isRealVsRealMatch = realPlayerIds.Contains(nextMatch.PlayerOneId)
            && realPlayerIds.Contains(nextMatch.PlayerTwoId);

        Assert.True(isRealVsRealMatch);
    }

    // Confirms bye auto-skip still works when bye ID metadata and BYE display labels are both missing.
    [Fact]
    public async Task GetCurrentMatchAsyncSkipsByeMatchesWhenByeMetadataAndLabelAreMissing()
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, TotalPlayers: 4));
        var users = await SetupDummyUsersAsync(3);
        var players = await SetupDummyPlayersAsync(users);

        for (var index = 0; index < players.Count; index++)
        {
            _gameService.AddPlayerToGame(players[index], gameId, users[index].Id);
        }

        var started = await _gameService.StartGameAsync(gameId);
        if (!started)
        {
            throw new InvalidOperationException("StartGameAsync failed for bye-metadata-and-label regression test.");
        }

        var gameServiceConcrete = _gameService as GameService;
        if (gameServiceConcrete is null)
        {
            throw new InvalidOperationException("Unable to access concrete GameService for bye-metadata-and-label regression setup.");
        }

        var bracketStatesField = typeof(GameService).GetField("_bracketStates", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bracketStatesField?.GetValue(gameServiceConcrete) is not IDictionary bracketStates)
        {
            throw new InvalidOperationException("Unable to access bracket runtime state dictionary for bye-metadata-and-label regression setup.");
        }

        if (!bracketStates.Contains(gameId))
        {
            throw new InvalidOperationException("Expected in-memory bracket runtime state for bye-metadata-and-label regression setup.");
        }

        var runtimeState = bracketStates[gameId];
        if (runtimeState is null)
        {
            throw new InvalidOperationException("Runtime bracket state instance was null in bye-metadata-and-label regression setup.");
        }

        var byePlayerIdsProperty = runtimeState.GetType().GetProperty("ByePlayerIds", BindingFlags.Instance | BindingFlags.Public);
        var byePlayerIds = byePlayerIdsProperty?.GetValue(runtimeState);
        var clearMethod = byePlayerIds?.GetType().GetMethod("Clear", Type.EmptyTypes);
        if (clearMethod is null)
        {
            throw new InvalidOperationException("Unable to clear bye metadata in runtime state for bye-metadata-and-label regression setup.");
        }

        clearMethod.Invoke(byePlayerIds, null);

        var playersProperty = runtimeState.GetType().GetProperty("Players", BindingFlags.Instance | BindingFlags.Public);
        if (playersProperty?.GetValue(runtimeState) is not IEnumerable runtimePlayers)
        {
            throw new InvalidOperationException("Unable to read runtime players for bye-metadata-and-label regression setup.");
        }

        foreach (var runtimePlayer in runtimePlayers)
        {
            if (runtimePlayer is null)
            {
                continue;
            }

            var userIdProperty = runtimePlayer.GetType().GetProperty("UserId", BindingFlags.Instance | BindingFlags.Public);
            var displayNameProperty = runtimePlayer.GetType().GetProperty("DisplayName", BindingFlags.Instance | BindingFlags.Public);
            var userId = userIdProperty?.GetValue(runtimePlayer) as string;

            if (!string.IsNullOrWhiteSpace(userId) && userId.Equals(AppConstants.ByeUserId, StringComparison.OrdinalIgnoreCase))
            {
                displayNameProperty?.SetValue(runtimePlayer, "NullObjectSlot");
            }
        }

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        if (currentMatch is null)
        {
            throw new InvalidOperationException("Current match should exist after bye auto-resolution in bye-metadata-and-label regression test.");
        }

        var realPlayerIds = players.Select(player => player.Id).ToHashSet();
        var isRealVsRealMatch = realPlayerIds.Contains(currentMatch.PlayerOneId)
            && realPlayerIds.Contains(currentMatch.PlayerTwoId);

        Assert.True(isRealVsRealMatch);
    }
}
