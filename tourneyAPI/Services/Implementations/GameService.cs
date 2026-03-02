namespace Services;

using System.Security.Claims;
using System.Text.Json;
using Contracts;
using Entities;
using Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Services.Brackets;

// Coordinates game lifecycle, bracket state transitions, and persistence.
public class GameService : IGameService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<Guid, BracketRuntimeState> _bracketStates = new();

    private readonly Dictionary<BracketMode, IBracketEngine> _bracketEngines = new()
    {
        [BracketMode.SINGLE_ELIMINATION] = new SingleEliminationEngine(),
        [BracketMode.DOUBLE_ELIMINATION] = new DoubleEliminationEngine()
    };

    public List<ApplicationUser> _userSessions;

    // Creates a game service with required dependencies and in-memory state stores.
    public GameService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

        var game = new Game
        {
            Id = Guid.NewGuid(),
            BracketMode = requestedMode
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

        var initializedState = bracketEngine.Initialize(gameId, game.currentPlayers);
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
        return true;
    }

    // Builds a bracket snapshot response for client display.
    public async Task<BracketSnapshotResponse?> GetBracketSnapshotAsync(Guid gameId)
    {
        var state = await GetOrHydrateBracketStateAsync(gameId);
        if (state is null)
        {
            return null;
        }

        if (!_bracketEngines.ContainsKey(state.Mode))
        {
            return null;
        }

        var bracketEngine = _bracketEngines[state.Mode];

        return bracketEngine.BuildSnapshot(state);
    }

    // Builds the current active match for gameplay.
    public async Task<CurrentMatchResponse?> GetCurrentMatchAsync(Guid gameId)
    {
        var state = await GetOrHydrateBracketStateAsync(gameId);
        if (state is null)
        {
            return null;
        }

        if (!_bracketEngines.ContainsKey(state.Mode))
        {
            return null;
        }

        var bracketEngine = _bracketEngines[state.Mode];

        return bracketEngine.BuildCurrentMatch(state);
    }

    // Returns the high-level game state used by the frontend state machine.
    public async Task<GameStateResponse?> GetGameStateAsync(Guid gameId)
    {
        var game = await GetGameByIdAsync(gameId);
        if (game is null)
        {
            return null;
        }

        var state = await GetOrHydrateBracketStateAsync(gameId);
        if (state is null)
        {
            return new GameStateResponse(
                gameId,
                GameState.LOBBY_WAITING,
                false,
                null,
                null,
                null);
        }

        if (!_bracketEngines.ContainsKey(state.Mode))
        {
            return new GameStateResponse(
                gameId,
                GameState.LOBBY_WAITING,
                false,
                null,
                null,
                null);
        }

            var bracketEngine = _bracketEngines[state.Mode];

        var currentMatch = bracketEngine.BuildCurrentMatch(state);
        var gameState = ResolveGameState(state, currentMatch);

        return new GameStateResponse(
            gameId,
            gameState,
            state.GameStarted,
            currentMatch?.MatchId,
            currentMatch?.PlayerOneId,
            currentMatch?.PlayerTwoId);
    }

            // Applies a reported match result and advances bracket state.
    public async Task<bool> ReportMatchResultAsync(Guid gameId, ReportMatchRequest request)
    {
        var state = await GetOrHydrateBracketStateAsync(gameId);
        if (state is null)
        {
            return false;
        }

        if (!_bracketEngines.ContainsKey(state.Mode))
        {
            return false;
        }

        var bracketEngine = _bracketEngines[state.Mode];

        var success = bracketEngine.TryReportMatch(state, request.MatchId, request.WinnerPlayerId);
        if (!success)
        {
            return false;
        }

        await PersistBracketStateAsync(gameId, state);

        var currentMatch = bracketEngine.BuildCurrentMatch(state);
        var gameState = ResolveGameState(state, currentMatch);
        Log.Information("Game {GameId} flow state after report => {FlowState}", gameId, gameState);

        return true;
    }

    // Returns in-memory bracket state or hydrates it from persisted JSON.
    private async Task<BracketRuntimeState?> GetOrHydrateBracketStateAsync(Guid gameId)
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
    private async Task PersistBracketStateAsync(Guid gameId, BracketRuntimeState state)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var game = await dbContext.Games.FindAsync(gameId);
        if (game is null)
        {
            throw new InvalidOperationException($"Unable to persist bracket state because game {gameId} was not found.");
        }

        game.BracketMode = state.Mode;
        game.BracketStateJson = JsonSerializer.Serialize(state);

        await dbContext.SaveChangesAsync();
    }

    // Resolves current frontend-facing game state from runtime data.
    private static GameState ResolveGameState(BracketRuntimeState state, CurrentMatchResponse? currentMatch)
    {
        if (!state.GameStarted)
        {
            return GameState.LOBBY_WAITING;
        }

        if (currentMatch is not null)
        {
            return GameState.IN_MATCH_ACTIVE;
        }

        var hasOpenMatches = state.Matches.Any(match =>
            match.Status == BracketMatchStatus.READY ||
            match.Status == BracketMatchStatus.IN_PROGRESS ||
            match.Status == BracketMatchStatus.PENDING);

        if (hasOpenMatches)
        {
            return GameState.BRACKET_VIEW;
        }

        if (state.Matches.Count > 0)
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
}
