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

    // Confirms a lone entrant still reaches a completed tournament, and is named
    // its winner, once every bye in a full-size bracket has been resolved.
    //
    // The existing real-vs-bye test uses a bracket of two, which is one match. A
    // host who asked for eight and got one player has to walk a bye through
    // several rounds - and, in double elimination, through a losers lane and a
    // grand final made entirely of byes - before anything can complete.
    [Theory]
    [InlineData(BracketMode.SINGLE_ELIMINATION)]
    [InlineData(BracketMode.DOUBLE_ELIMINATION)]
    public async Task StartGameAsyncWithLoneEntrantCompletesAndCrownsThatPlayer(BracketMode bracketMode)
    {
        var gameId = await _gameService.CreateGame(new CreateGameOptions(bracketMode, TotalPlayers: 8));
        var users = await SetupDummyUsersAsync(1);
        var players = await SetupDummyPlayersAsync(users);

        _gameService.AddPlayerToGame(players[0], gameId, users[0].Id);

        var started = await _gameService.StartGameAsync(gameId);
        Assert.True(started, $"StartGameAsync failed for lone-entrant {bracketMode} test.");

        var currentMatch = await _gameService.GetCurrentMatchAsync(gameId);
        var flowState = await _gameService.GetGameStateAsync(gameId);
        var snapshot = await _gameService.GetBracketSnapshotAsync(gameId);

        Assert.Null(currentMatch);
        Assert.NotNull(flowState);
        Assert.Equal(GameState.COMPLETE, flowState!.State);
        Assert.NotNull(snapshot);

        // No match may be left undecided, or the client would have nothing to
        // declare and would sit on the bracket forever.
        var undecided = snapshot!.Matches
            .Where(match => match.Status != BracketMatchStatus.COMPLETE)
            .ToList();

        Assert.True(
            undecided.Count == 0,
            $"{undecided.Count} match(es) left undecided for {bracketMode}: " +
            string.Join(", ", undecided.Select(match => $"{match.Lane} r{match.Round} #{match.MatchNumber} {match.Status}")));

        // The terminal match is the one no winner advances out of. Its winner is
        // the champion the client shows, so it has to be the real entrant rather
        // than one of the byes that filled the bracket.
        var terminalMatch = snapshot.Matches.Single(match => match.NextMatchForWinner is null);

        Assert.Equal(players[0].Id, terminalMatch.WinnerId);
    }

    // Confirms one real participant vote can commit real-vs-bye matches when bye metadata is missing in runtime state.
    [Fact]
    public async Task SubmitMatchVoteAsyncCommitsRealVersusByeWhenByeMetadataIsMissing()
    {
        // Five players in a bracket of eight, which leaves bye matches still
        // standing once the game has started.
        //
        // Bye auto-resolution walks matches in play order and stops at the first
        // one it cannot decide, so a bye sitting behind a real-vs-real match
        // survives startup. Three players in a bracket of four no longer works
        // for this: standard seeding puts the bye on the top seed, so it is the
        // very first match and is consumed before the test can strip the
        // metadata. The behavior under test is unchanged — this only needs a
        // shape where an undecided bye match is still reachable.
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

        // Walks to the real-vs-bye match, committing any real-vs-real match met
        // on the way.
        //
        // Which opening match holds the bye depends on seeding. Standard seed
        // order pairs the top seed against the bottom slot, and GameService
        // appends synthetic byes last, so the bye match can legitimately come
        // first. The behavior under test — one real vote committing a bye match
        // when bye metadata is missing — does not depend on that ordering, so
        // this searches for the match rather than assuming its position.
        CurrentMatchResponse? realVersusByeMatch = null;
        var realVersusByeParticipants = new List<Player>();

        for (var attempt = 0; attempt < 4 && realVersusByeMatch is null; attempt++)
        {
            var candidateMatch = await _gameService.GetCurrentMatchAsync(gameId);
            if (candidateMatch is null)
            {
                break;
            }

            var candidateRealParticipants = players
                .Where(player => player.Id == candidateMatch.PlayerOneId || player.Id == candidateMatch.PlayerTwoId)
                .ToList();

            if (candidateRealParticipants.Count == 1)
            {
                realVersusByeMatch = candidateMatch;
                realVersusByeParticipants = candidateRealParticipants;
                break;
            }

            if (candidateRealParticipants.Count != 2)
            {
                throw new InvalidOperationException("Expected an opening match with at least one real participant in regression test.");
            }

            var candidateVoteRequest = new SubmitMatchVoteRequest(candidateMatch.MatchId, candidateMatch.PlayerOneId);
            var candidatePlayerOneUserId = players.First(player => player.Id == candidateMatch.PlayerOneId).UserId;
            var candidatePlayerTwoUserId = players.First(player => player.Id == candidateMatch.PlayerTwoId).UserId;

            var pendingVote = await _gameService.SubmitMatchVoteAsync(gameId, candidatePlayerOneUserId, candidateVoteRequest);
            if (pendingVote.Status != SubmitMatchVoteStatus.PENDING)
            {
                throw new InvalidOperationException("First vote should be pending for a real-vs-real match in regression test.");
            }

            var committingVote = await _gameService.SubmitMatchVoteAsync(gameId, candidatePlayerTwoUserId, candidateVoteRequest);
            if (committingVote.Status != SubmitMatchVoteStatus.COMMITTED)
            {
                throw new InvalidOperationException("Second vote should commit a real-vs-real match in regression test.");
            }
        }

        if (realVersusByeMatch is null || realVersusByeParticipants.Count != 1)
        {
            throw new InvalidOperationException("Expected a real-vs-bye match in regression test.");
        }

        var secondMatch = realVersusByeMatch;
        var realParticipant = realVersusByeParticipants[0];
        var byeParticipantId = secondMatch.PlayerOneId == realParticipant.Id
            ? secondMatch.PlayerTwoId
            : secondMatch.PlayerOneId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var persistedByePlayer = await dbContext.Players.FindAsync(byeParticipantId);

            if (persistedByePlayer is null)
            {
                dbContext.Players.Add(new Player
                {
                    Id = byeParticipantId,
                    UserId = AppConstants.ByeUserId,
                    DisplayName = "PersistedBye",
                    CurrentGameID = gameId,
                    CurrentCharacter = new Character()
                });
            }
            else
            {
                persistedByePlayer.UserId = AppConstants.ByeUserId;
            }

            await dbContext.SaveChangesAsync();
        }

        var secondMatchVoteRequest = new SubmitMatchVoteRequest(secondMatch.MatchId, realParticipant.Id);

        var realVersusByeVote = await _gameService.SubmitMatchVoteAsync(gameId, realParticipant.UserId, secondMatchVoteRequest);

        var committedWithSingleVote = realVersusByeVote.Status == SubmitMatchVoteStatus.COMMITTED
            && realVersusByeVote.VoteCount == 1
            && realVersusByeVote.CommittedWinnerPlayerId == realParticipant.Id;

        Assert.True(committedWithSingleVote);
    }
}
