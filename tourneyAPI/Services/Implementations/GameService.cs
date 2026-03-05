namespace Services;

using System.Security.Claims;
using System.Text.Json;
using Contracts;
using Entities;
using Enums;
using Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Services.Brackets;

// Coordinates game lifecycle, bracket state transitions, and persistence.
public class GameService : IGameService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Guid, Guid, Guid> _selectByeVsByeWinner;

    private readonly Dictionary<Guid, BracketRuntimeState> _bracketStates = new();
    private readonly Dictionary<Guid, Dictionary<Guid, Dictionary<string, Guid>>> _matchVoteLedger = new();
    private readonly SemaphoreSlim _voteLedgerGate = new(1, 1);

    private readonly Dictionary<BracketMode, IBracketEngine> _bracketEngines = new()
    {
        [BracketMode.SINGLE_ELIMINATION] = new SingleEliminationEngine(),
        [BracketMode.DOUBLE_ELIMINATION] = new DoubleEliminationEngine()
    };

    public List<ApplicationUser> _userSessions;

    // Creates a game service with required dependencies and in-memory state stores.
    public GameService(
        IServiceProvider serviceProvider,
        Func<Guid, Guid, Guid>? selectByeVsByeWinner = null)
    {
        _serviceProvider = serviceProvider;
        _selectByeVsByeWinner = selectByeVsByeWinner ?? ((playerOneId, playerTwoId) =>
            Random.Shared.Next(0, 2) == 0 ? playerOneId : playerTwoId);
        _userSessions = new List<ApplicationUser>();
    }

    // Creates a new game with default bracket mode.
    public async Task<Guid> CreateGame()
    {
        Log.Information("CreateGame");

        var game = new Game
        {
            Id = Guid.NewGuid(),
            BracketMode = BracketMode.SINGLE_ELIMINATION
        };

        await InsertGameAsync(game);

        Log.Information("New game created with gameId {GameId}", game.Id);
        return game.Id;
    }

    // Creates a new game with caller-specified options.
    public async Task<Guid> CreateGame(CreateGameOptions options)
    {
        var requestedMode = options?.BracketMode ?? BracketMode.SINGLE_ELIMINATION;
        var requestedTotalPlayers = options?.TotalPlayers ?? 0;

        var game = new Game
        {
            Id = Guid.NewGuid(),
            BracketMode = requestedMode,
            byes = requestedTotalPlayers
        };

        await InsertGameAsync(game);

        Log.Information("New game created with gameId {GameId} and mode {BracketMode}", game.Id, requestedMode);
        return game.Id;
    }

    // Persists a newly created game record.
    private async Task InsertGameAsync(Game game)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();
    }

    // Persists current runtime bracket state for a specific game.
    public async Task UpdateGameAsync(Guid gameId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dbGame = await dbContext.Games.FindAsync(gameId);
        if (dbGame is null)
        {
            throw new InvalidOperationException($"Game with ID {gameId} does not exist.");
        }

        if (_bracketStates.ContainsKey(gameId))
        {
            var bracketState = _bracketStates[gameId];
            dbGame.BracketMode = bracketState.Mode;
            dbGame.BracketStateJson = JsonSerializer.Serialize(bracketState);
        }

        await dbContext.SaveChangesAsync();
    }

    // Loads a game's persisted bracket runtime state into memory.
    public async Task<bool> LoadGameAsync(Guid gameId)
    {
        Log.Information("Load game {GameId}", gameId);

        var game = await GetGameByIdAsync(gameId);
        if (game is null)
        {
            Log.Warning("Unable to load game {GameId} because it does not exist", gameId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(game.BracketStateJson))
        {
            return true;
        }

        try
        {
            var hydratedState = JsonSerializer.Deserialize<BracketRuntimeState>(game.BracketStateJson);
            if (hydratedState is not null)
            {
                _bracketStates[gameId] = hydratedState;
            }
        }
        catch (JsonException exception)
        {
            Log.Warning(exception, "Unable to hydrate bracket runtime state for game {GameId}", gameId);
            return false;
        }

        return true;
    }

    // Returns all games with their currently assigned players.
    public async Task<List<Game>?> GetAllGamesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var games = await dbContext.Games.ToListAsync();
        if (games.Count == 0)
        {
            Log.Warning("GetAllGames returned zero games");
            return null;
        }

        foreach (var game in games)
        {
            game.currentPlayers = await dbContext.Players
                .Include(player => player.CurrentCharacter)
                .Where(player => player.CurrentGameID == game.Id)
                .ToListAsync();
        }

        return games;
    }

    // Returns a single game and its currently assigned players.
    public async Task<Game?> GetGameByIdAsync(Guid gameId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var game = await dbContext.Games.FirstOrDefaultAsync(currentGame => currentGame.Id == gameId);
        if (game is null)
        {
            return null;
        }

        game.currentPlayers = await dbContext.Players
            .Include(player => player.CurrentCharacter)
            .Where(player => player.CurrentGameID == gameId)
            .ToListAsync();

        return game;
    }

    // Returns players assigned to a specific game.
    public async Task<List<Player>> GetPlayersInGame(Guid gameId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Players
            .Include(player => player.CurrentCharacter)
            .Where(player => player.CurrentGameID == gameId)
            .ToListAsync();
    }

    // Creates or refreshes an in-memory user session entry.
    public bool CreateUserSession(ApplicationUser addUser)
    {
        if (addUser is null)
        {
            Log.Error("Unable to add user session because user payload is null");
            return false;
        }

        var existingSession = _userSessions.FirstOrDefault(user => user.Id == addUser.Id);
        if (existingSession is not null)
        {
            existingSession.UserName = addUser.UserName;
            existingSession.Email = addUser.Email;
            return true;
        }

        _userSessions.Add(addUser);
        Log.Information("User {UserName} added to in-memory user sessions", addUser.UserName);
        return true;
    }

    // Removes an in-memory user session entry.
    public bool EndUserSession(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            Log.Error("Unable to end session because userId could not be parsed from ClaimsPrincipal");
            return false;
        }

        var session = _userSessions.FirstOrDefault(current => current.Id == userId);
        if (session is null)
        {
            Log.Warning("Unable to end session. User {UserId} was not found in user sessions", userId);
            return false;
        }

        _userSessions.Remove(session);
        return true;
    }

    // Adds a player to a game or updates existing player assignment.
    public bool AddPlayerToGame(Player player, Guid gameId, string userId)
    {
        if (player is null)
        {
            Log.Warning("Rejected AddPlayer for game {GameId} because player payload is null", gameId);
            return false;
        }

        if (player.CurrentCharacter is null)
        {
            Log.Warning("Rejected AddPlayer for game {GameId} because player character payload is null", gameId);
            return false;
        }

        if (_bracketStates.ContainsKey(gameId))
        {
            var runtimeState = _bracketStates[gameId];
            if (runtimeState.GameStarted)
            {
                Log.Warning("Rejected AddPlayer for game {GameId} because game has already started", gameId);
                return false;
            }
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var game = dbContext.Games.FirstOrDefault(currentGame => currentGame.Id == gameId);

        if (game is null)
        {
            Log.Warning("AddPlayer failed. Game {GameId} was not found", gameId);
            return false;
        }

        Guid resolvedPlayerId;
        if (player.Id == Guid.Empty)
        {
            resolvedPlayerId = Guid.NewGuid();
        }
        else
        {
            resolvedPlayerId = player.Id;
        }

        var allPlayers = dbContext.Players
            .Include(currentPlayer => currentPlayer.CurrentCharacter)
            .ToList();

        Player? existingPlayer = null;
        foreach (var currentPlayer in allPlayers)
        {
            if (currentPlayer.Id == resolvedPlayerId)
            {
                existingPlayer = currentPlayer;
                break;
            }
        }

        if (existingPlayer is not null)
        {
            existingPlayer.UserId = userId;
            existingPlayer.DisplayName = player.DisplayName;
            existingPlayer.CurrentScore = player.CurrentScore;
            existingPlayer.CurrentRound = player.CurrentRound;
            existingPlayer.CurrentGameID = gameId;

            if (existingPlayer.CurrentCharacter is null)
            {
                existingPlayer.CurrentCharacter = CreateDetachedCharacter(player.CurrentCharacter);
            }
            else
            {
                var sourceCharacter = player.CurrentCharacter;
                existingPlayer.CurrentCharacter.characterName = sourceCharacter.characterName;
                existingPlayer.CurrentCharacter.archetype = sourceCharacter.archetype;
                existingPlayer.CurrentCharacter.fallSpeed = sourceCharacter.fallSpeed;
                existingPlayer.CurrentCharacter.tierPlacement = sourceCharacter.tierPlacement;
                existingPlayer.CurrentCharacter.weightClass = sourceCharacter.weightClass;
            }

            dbContext.SaveChanges();
            return true;
        }

        var playerToAdd = new Player
        {
            Id = resolvedPlayerId,
            UserId = userId,
            DisplayName = player.DisplayName,
            CurrentScore = player.CurrentScore,
            CurrentRound = player.CurrentRound,
            CurrentGameID = gameId,
            CurrentCharacter = CreateDetachedCharacter(player.CurrentCharacter)
        };

        dbContext.Players.Add(playerToAdd);

        dbContext.SaveChanges();
        return true;
    }

    // Starts bracket processing for a game.
    public async Task<bool> StartGameAsync(Guid gameId)
    {
        Log.Information("Start game {GameId}", gameId);

        var game = await GetGameByIdAsync(gameId);
        if (game is null)
        {
            Log.Warning("StartGame failed because game {GameId} was not found", gameId);
            return false;
        }

        if (!_bracketEngines.ContainsKey(game.BracketMode))
        {
            Log.Error("No bracket engine registered for mode {BracketMode}", game.BracketMode);
            return false;
        }

        var bracketEngine = _bracketEngines[game.BracketMode];

        var targetBracketSize = NormalizeBracketSize(game.byes, game.currentPlayers.Count);
        var seededPlayers = BuildSeededPlayersWithByes(game.currentPlayers, gameId, targetBracketSize);

        var initializedState = bracketEngine.Initialize(gameId, seededPlayers);
        initializedState.ByePlayerIds = seededPlayers
            .Where(player => player.UserId == AppConstants.ByeUserId)
            .Select(player => player.Id)
            .ToHashSet();

        _matchVoteLedger.Remove(gameId);

        await AutoResolveByeMatchesAsync(gameId, initializedState, bracketEngine, persistChanges: false);
        _bracketStates[gameId] = initializedState;

        await PersistBracketStateAsync(gameId, initializedState);

        var currentMatch = bracketEngine.BuildCurrentMatch(initializedState);
        var gameState = ResolveGameState(initializedState, currentMatch);
        Log.Information("Game {GameId} flow state after start => {FlowState}", gameId, gameState);

        return true;
    }

    // Deletes a game and all of its associated players.
    public bool EndGame(Guid gameId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var game = dbContext.Games.FirstOrDefault(currentGame => currentGame.Id == gameId);
        if (game is null)
        {
            return false;
        }

        var gamePlayers = dbContext.Players.Where(player => player.CurrentGameID == gameId).ToList();
        if (gamePlayers.Count > 0)
        {
            dbContext.Players.RemoveRange(gamePlayers);
        }

        dbContext.Games.Remove(game);
        dbContext.SaveChanges();

        _bracketStates.Remove(gameId);
        _matchVoteLedger.Remove(gameId);
        return true;
    }

    // Builds a bracket snapshot response for client display.
    public async Task<BracketSnapshotResponse?> GetBracketSnapshotAsync(Guid gameId)
    {
        var bracketRuntimeState = await HydrateBracketStateAsync(gameId);
        if (bracketRuntimeState is null)
        {
            return null;
        }

        if (!_bracketEngines.ContainsKey(bracketRuntimeState.Mode))
        {
            return null;
        }

        var bracketEngine = _bracketEngines[bracketRuntimeState.Mode];

        await AutoResolveByeMatchesAsync(gameId, bracketRuntimeState, bracketEngine, persistChanges: true);

        return bracketEngine.BuildSnapshot(bracketRuntimeState);
    }

    // Builds the current active match for gameplay.
    public async Task<CurrentMatchResponse?> GetCurrentMatchAsync(Guid gameId)
    {
        var bracketRuntimeState = await HydrateBracketStateAsync(gameId);
        if (bracketRuntimeState is null)
        {
            return null;
        }

        if (!_bracketEngines.ContainsKey(bracketRuntimeState.Mode))
        {
            return null;
        }

        var bracketEngine = _bracketEngines[bracketRuntimeState.Mode];

        await AutoResolveByeMatchesAsync(gameId, bracketRuntimeState, bracketEngine, persistChanges: true);

        return bracketEngine.BuildCurrentMatch(bracketRuntimeState);
    }

    // Returns the high-level game state used by the frontend state machine.
    public async Task<GameStateResponse?> GetGameStateAsync(Guid gameId)
    {
        var game = await GetGameByIdAsync(gameId);
        if (game is null)
        {
            return null;
        }

        var bracketRuntimeState = await HydrateBracketStateAsync(gameId);
        if (bracketRuntimeState is null)
        {
            return new GameStateResponse(
                gameId,
                GameState.LOBBY_WAITING,
                false,
                null,
                null,
                null);
        }

        if (!_bracketEngines.ContainsKey(bracketRuntimeState.Mode))
        {
            return new GameStateResponse(
                gameId,
                GameState.LOBBY_WAITING,
                false,
                null,
                null,
                null);
        }

        var bracketEngine = _bracketEngines[bracketRuntimeState.Mode];

        await AutoResolveByeMatchesAsync(gameId, bracketRuntimeState, bracketEngine, persistChanges: true);

        var currentMatch = bracketEngine.BuildCurrentMatch(bracketRuntimeState);
        var gameState = ResolveGameState(bracketRuntimeState, currentMatch);

        return new GameStateResponse(
            gameId,
            gameState,
            bracketRuntimeState.GameStarted,
            currentMatch?.MatchId,
            currentMatch?.PlayerOneId,
            currentMatch?.PlayerTwoId);
    }

    // Applies a reported match result and advances bracket state.
    public async Task<bool> ReportMatchResultAsync(Guid gameId, ReportMatchRequest request)
    {
        var bracketRuntimeState = await HydrateBracketStateAsync(gameId);
        if (bracketRuntimeState is null)
        {
            return false;
        }

        if (!_bracketEngines.ContainsKey(bracketRuntimeState.Mode))
        {
            return false;
        }

        var bracketEngine = _bracketEngines[bracketRuntimeState.Mode];

        var success = bracketEngine.TryReportMatch(bracketRuntimeState, request.MatchId, request.WinnerPlayerId);
        if (!success)
        {
            return false;
        }

        RemoveLedgerForMatch(gameId, request.MatchId);

        await AutoResolveByeMatchesAsync(gameId, bracketRuntimeState, bracketEngine, persistChanges: false);

        await PersistBracketStateAsync(gameId, bracketRuntimeState);

        var currentMatch = bracketEngine.BuildCurrentMatch(bracketRuntimeState);
        var gameState = ResolveGameState(bracketRuntimeState, currentMatch);
        Log.Information("Game {GameId} flow state after report => {FlowState}", gameId, gameState);

        return true;
    }

    // Accepts one authenticated player vote and commits match when both players agree on winner.
    public async Task<SubmitMatchVoteResponse> SubmitMatchVoteAsync(Guid gameId, string voterUserId, SubmitMatchVoteRequest request)
    {
        var validationResult = await ValidateVoteSubmissionAsync(gameId, voterUserId, request);
        if (validationResult.FailureResponse is not null)
        {
            return validationResult.FailureResponse;
        }

        var ledgerResponse = await RecordVoteAsync(
            gameId,
            request.MatchId,
            voterUserId,
            request.WinnerPlayerId,
            validationResult.RequiredConsensusVotes);

        if (ledgerResponse is not null)
        {
            return ledgerResponse;
        }

        var applied = await ReportMatchResultAsync(gameId, new ReportMatchRequest(request.MatchId, request.WinnerPlayerId));
        if (!applied)
        {
            return new SubmitMatchVoteResponse(
                gameId,
                request.MatchId,
                SubmitMatchVoteStatus.APPLY_FAILED,
                validationResult.RequiredConsensusVotes,
                null);
        }

        return new SubmitMatchVoteResponse(
            gameId,
            request.MatchId,
            SubmitMatchVoteStatus.COMMITTED,
            validationResult.RequiredConsensusVotes,
            request.WinnerPlayerId);
    }

    // Validates vote payload and derives required participant vote count for match consensus.
    private async Task<(SubmitMatchVoteResponse? FailureResponse, int RequiredConsensusVotes)> ValidateVoteSubmissionAsync(
        Guid gameId,
        string voterUserId,
        SubmitMatchVoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(voterUserId))
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.VOTER_NOT_PARTICIPANT, 0, null), 0);
        }

        var bracketRuntimeState = await HydrateBracketStateAsync(gameId);
        if (bracketRuntimeState is null)
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.GAME_NOT_FOUND, 0, null), 0);
        }

        if (!bracketRuntimeState.GameStarted)
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.BRACKET_NOT_STARTED, 0, null), 0);
        }

        if (!_bracketEngines.ContainsKey(bracketRuntimeState.Mode))
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.BRACKET_NOT_STARTED, 0, null), 0);
        }

        var bracketEngine = _bracketEngines[bracketRuntimeState.Mode];
        await AutoResolveByeMatchesAsync(gameId, bracketRuntimeState, bracketEngine, persistChanges: true);

        var activeMatch = bracketEngine.BuildCurrentMatch(bracketRuntimeState);
        if (activeMatch is null || activeMatch.MatchId != request.MatchId)
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.MATCH_NOT_ACTIVE, 0, null), 0);
        }

        var winnerIsPlayerOne = activeMatch.PlayerOneId == request.WinnerPlayerId;
        var winnerIsPlayerTwo = activeMatch.PlayerTwoId == request.WinnerPlayerId;
        if (!winnerIsPlayerOne && !winnerIsPlayerTwo)
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.INVALID_WINNER, 0, null), 0);
        }

        var matchParticipantUserIds = await GetPlayerUserIdsAsync(
            gameId,
            new[] { activeMatch.PlayerOneId, activeMatch.PlayerTwoId });

        var isParticipantVoter = matchParticipantUserIds.Values.Any(userId => userId == voterUserId);
        if (!isParticipantVoter)
        {
            return (new SubmitMatchVoteResponse(gameId, request.MatchId, SubmitMatchVoteStatus.VOTER_NOT_PARTICIPANT, 0, null), 0);
        }

        var requiredConsensusVotes = ResolveRequiredConsensusVotes(matchParticipantUserIds, activeMatch);
        return (null, requiredConsensusVotes);
    }

    // Stores one vote and determines whether match consensus has been reached.
    private async Task<SubmitMatchVoteResponse?> RecordVoteAsync(
        Guid gameId,
        Guid matchId,
        string voterUserId,
        Guid votedWinnerPlayerId,
        int requiredConsensusVotes)
    {
        await _voteLedgerGate.WaitAsync();
        try
        {
            if (!_matchVoteLedger.ContainsKey(gameId))
            {
                _matchVoteLedger[gameId] = new Dictionary<Guid, Dictionary<string, Guid>>();
            }

            var gameVotes = _matchVoteLedger[gameId];
            if (!gameVotes.ContainsKey(matchId))
            {
                gameVotes[matchId] = new Dictionary<string, Guid>();
            }

            var matchVotes = gameVotes[matchId];
            if (matchVotes.ContainsKey(voterUserId))
            {
                return new SubmitMatchVoteResponse(gameId, matchId, SubmitMatchVoteStatus.DUPLICATE_VOTE, matchVotes.Count, null);
            }

            matchVotes[voterUserId] = votedWinnerPlayerId;

            var distinctWinners = matchVotes.Values.Distinct().Count();
            if (distinctWinners > 1)
            {
                matchVotes.Clear();
                return new SubmitMatchVoteResponse(gameId, matchId, SubmitMatchVoteStatus.CONFLICT, 0, null);
            }

            if (matchVotes.Count < requiredConsensusVotes)
            {
                return new SubmitMatchVoteResponse(gameId, matchId, SubmitMatchVoteStatus.PENDING, matchVotes.Count, null);
            }

            return null;
        }
        finally
        {
            _voteLedgerGate.Release();
        }
    }

    // Resolves required consensus votes based on authenticated participants in the active match.
    private static int ResolveRequiredConsensusVotes(
        Dictionary<Guid, string> matchParticipantUserIds,
        CurrentMatchResponse activeMatch)
    {
        var participantUserIds = new List<string>();

        if (matchParticipantUserIds.TryGetValue(activeMatch.PlayerOneId, out var playerOneUserId) && !string.IsNullOrWhiteSpace(playerOneUserId))
        {
            participantUserIds.Add(playerOneUserId);
        }

        if (matchParticipantUserIds.TryGetValue(activeMatch.PlayerTwoId, out var playerTwoUserId) && !string.IsNullOrWhiteSpace(playerTwoUserId))
        {
            participantUserIds.Add(playerTwoUserId);
        }

        var uniqueParticipantCount = participantUserIds.Distinct().Count();
        return Math.Clamp(uniqueParticipantCount, 1, 2);
    }

    // Returns in-memory bracket state or hydrates it from persisted JSON.
    private async Task<BracketRuntimeState?> HydrateBracketStateAsync(Guid gameId)
    {
        if (_bracketStates.ContainsKey(gameId))
        {
            var existingState = _bracketStates[gameId];
            return existingState;
        }

        var game = await GetGameByIdAsync(gameId);
        if (game is null || string.IsNullOrWhiteSpace(game.BracketStateJson))
        {
            return null;
        }

        try
        {
            var hydratedState = JsonSerializer.Deserialize<BracketRuntimeState>(game.BracketStateJson);
            if (hydratedState is null)
            {
                return null;
            }

            _bracketStates[gameId] = hydratedState;
            return hydratedState;
        }
        catch (JsonException exception)
        {
            Log.Warning(exception, "Unable to hydrate bracket runtime state for game {GameId}", gameId);
            return null;
        }
    }

    // Persists runtime bracket state to the game record.
    private async Task PersistBracketStateAsync(Guid gameId, BracketRuntimeState bracketRuntimeState)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var game = await dbContext.Games.FindAsync(gameId);
        if (game is null)
        {
            throw new InvalidOperationException($"Unable to persist bracket state because game {gameId} was not found.");
        }

        game.BracketMode = bracketRuntimeState.Mode;
        game.BracketStateJson = JsonSerializer.Serialize(bracketRuntimeState);

        await dbContext.SaveChangesAsync();
    }

    // Resolves current frontend-facing game state from runtime data.
    private static GameState ResolveGameState(BracketRuntimeState bracketRuntimeState, CurrentMatchResponse? currentMatch)
    {
        if (!bracketRuntimeState.GameStarted)
        {
            return GameState.LOBBY_WAITING;
        }

        if (currentMatch is not null)
        {
            return GameState.IN_MATCH_ACTIVE;
        }

        var hasOpenMatches = bracketRuntimeState.Matches.Any(match =>
            match.Status == BracketMatchStatus.READY ||
            match.Status == BracketMatchStatus.IN_PROGRESS ||
            match.Status == BracketMatchStatus.PENDING);

        if (hasOpenMatches)
        {
            return GameState.BRACKET_VIEW;
        }

        if (bracketRuntimeState.Matches.Count > 0)
        {
            return GameState.COMPLETE;
        }

        return GameState.LOBBY_WAITING;
    }

    // Creates a detached character entity for safe persistence.
    private static Character CreateDetachedCharacter(Character sourceCharacter)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            characterName = sourceCharacter.characterName,
            archetype = sourceCharacter.archetype,
            fallSpeed = sourceCharacter.fallSpeed,
            tierPlacement = sourceCharacter.tierPlacement,
            weightClass = sourceCharacter.weightClass
        };
    }

    // Fills missing bracket slots with synthetic bye players for deterministic bracket sizing.
    private static List<Player> BuildSeededPlayersWithByes(
        IReadOnlyList<Player> realPlayers,
        Guid gameId,
        int targetBracketSize)
    {
        var seededPlayers = realPlayers
            .Where(player => player.Id != Guid.Empty)
            .DistinctBy(player => player.Id)
            .ToList();

        var byeSlotsToAdd = Math.Max(0, targetBracketSize - seededPlayers.Count);
        for (var byeIndex = 0; byeIndex < byeSlotsToAdd; byeIndex++)
        {
            seededPlayers.Add(new Player
            {
                Id = Guid.NewGuid(),
                UserId = AppConstants.ByeUserId,
                DisplayName = $"BYE {byeIndex + 1}",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentGameID = gameId,
                CurrentCharacter = new Character()
            });
        }

        return seededPlayers;
    }

    // Resolves bracket size as a power-of-two based on requested and current participants.
    private static int NormalizeBracketSize(int requestedTotalPlayers, int currentPlayers)
    {
        var normalizedRequested = RoundUpToPowerOfTwo(Math.Max(2, requestedTotalPlayers));
        var normalizedCurrent = RoundUpToPowerOfTwo(Math.Max(2, currentPlayers));
        return Math.Max(normalizedRequested, normalizedCurrent);
    }

    // Rounds a positive integer up to the nearest power-of-two.
    private static int RoundUpToPowerOfTwo(int value)
    {
        var result = 1;
        while (result < value)
        {
            result *= 2;
        }

        return result;
    }

    // Auto-completes bye matches so real players are advanced without manual voting.
    private async Task AutoResolveByeMatchesAsync(
        Guid gameId,
        BracketRuntimeState bracketRuntimeState,
        IBracketEngine bracketEngine,
        bool persistChanges)
    {
        var changed = false;
        var autoResolvedCount = 0;
        var maxAutoResolves = Math.Max(16, bracketRuntimeState.Matches.Count + (bracketRuntimeState.Players.Count * 2));

        while (true)
        {
            if (autoResolvedCount >= maxAutoResolves)
            {
                Log.Warning(
                    "Auto-resolve bye matches reached safety cap for game {GameId}. Resolved={ResolvedCount}, Cap={Cap}",
                    gameId,
                    autoResolvedCount,
                    maxAutoResolves);
                break;
            }

            var currentMatch = bracketEngine.BuildCurrentMatch(bracketRuntimeState);
            if (currentMatch is null)
            {
                break;
            }

            var winnerId = ResolveAutomaticWinner(bracketRuntimeState, currentMatch.PlayerOneId, currentMatch.PlayerTwoId);
            if (winnerId is null)
            {
                break;
            }

            var applied = bracketEngine.TryReportMatch(bracketRuntimeState, currentMatch.MatchId, winnerId.Value);
            if (!applied)
            {
                break;
            }

            changed = true;
            autoResolvedCount += 1;
        }

        if (changed && persistChanges)
        {
            await PersistBracketStateAsync(gameId, bracketRuntimeState);
        }
    }

    // Returns a forced winner only for bye-involved matches; returns null for real-vs-real matches.
    private Guid? ResolveAutomaticWinner(BracketRuntimeState bracketRuntimeState, Guid playerOneId, Guid playerTwoId)
    {
        var playerOneIsBye = bracketRuntimeState.ByePlayerIds.Contains(playerOneId);
        var playerTwoIsBye = bracketRuntimeState.ByePlayerIds.Contains(playerTwoId);

        if (!playerOneIsBye && !playerTwoIsBye)
        {
            return null;
        }

        if (playerOneIsBye && !playerTwoIsBye)
        {
            return playerTwoId;
        }

        if (playerTwoIsBye && !playerOneIsBye)
        {
            return playerOneId;
        }

        return _selectByeVsByeWinner(playerOneId, playerTwoId);
    }

    // Returns a map of participant player ids to user ids for authenticated vote validation.
    private async Task<Dictionary<Guid, string>> GetPlayerUserIdsAsync(Guid gameId, IEnumerable<Guid> playerIds)
    {
        var uniquePlayerIds = playerIds.Distinct().ToList();
        if (uniquePlayerIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var players = await dbContext.Players
            .Where(player => player.CurrentGameID == gameId && uniquePlayerIds.Contains(player.Id))
            .ToListAsync();

        return players.ToDictionary(player => player.Id, player => player.UserId);
    }

    // Removes cached votes for one match after resolution.
    private void RemoveLedgerForMatch(Guid gameId, Guid matchId)
    {
        if (!_matchVoteLedger.ContainsKey(gameId))
        {
            return;
        }

        var gameVotes = _matchVoteLedger[gameId];
        gameVotes.Remove(matchId);

        if (gameVotes.Count == 0)
        {
            _matchVoteLedger.Remove(gameId);
        }
    }
}
