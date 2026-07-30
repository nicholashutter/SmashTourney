namespace Services;

using System.Security.Claims;
using Contracts;
using Entities;

// Defines game lifecycle and bracket progression operations used by API routes.
public interface IGameService
{
    // Creates a new game using default settings.
    Task<Guid> CreateGame();

    // Creates a new game with explicit bracket options, recording its host.
    Task<Guid> CreateGame(CreateGameOptions options, string? hostUserId = null);

    // Resolves a returning user's identity and place in a game, or null when
    // that user has no player in it.
    Task<PlayerSessionResponse?> GetPlayerSessionAsync(Guid gameId, string userId);

    // Ends an existing game and clears its runtime state.
    bool EndGame(Guid endGameId);

    // Ends a game on behalf of a specific user, refusing anyone but its host.
    Task<EndGameStatus> EndGameAsync(Guid gameId, string requestingUserId);

    // Returns a lightweight description of the games one user belongs to.
    Task<List<GameSummaryResponse>> GetGameSummariesAsync(string userId);

    // Reports whether a user plays in or hosts a game. False both for an
    // outsider and for a game that does not exist, so a caller cannot use this
    // to tell those two apart.
    Task<bool> IsUserInGameAsync(Guid gameId, string userId);

    // Reports whether a user hosts a game.
    Task<bool> IsUserHostOfGameAsync(Guid gameId, string userId);

    // Deletes games that are finished or abandoned, returning how many went.
    Task<int> PruneStaleGamesAsync();

    // Returns one game by identifier.
    Task<Game?> GetGameByIdAsync(Guid gameId);

    // Returns players currently assigned to the game.
    Task<List<Player>> GetPlayersInGame(Guid gameId);

    // Returns all active games.
    Task<List<Game>?> GetAllGamesAsync();

    // Creates an in-memory user session after successful sign-in.
    bool CreateUserSession(ApplicationUser addUser);

    // Ends an in-memory user session.
    bool EndUserSession(ClaimsPrincipal user);

    // Adds or updates a player in a game.
    bool AddPlayerToGame(Player player, Guid gameId, string userId);

    // Starts tournament progression for a game.
    Task<bool> StartGameAsync(Guid existingGameId);

    // Loads persisted bracket state into runtime memory.
    Task<bool> LoadGameAsync(Guid gameId);

    // Persists current game state to storage.
    Task UpdateGameAsync(Guid gameId);

    // Returns a bracket snapshot for client rendering.
    Task<BracketSnapshotResponse?> GetBracketSnapshotAsync(Guid gameId);

    // Returns the current active match for a game.
    Task<CurrentMatchResponse?> GetCurrentMatchAsync(Guid gameId);

    // Returns the high-level game progression state.
    Task<GameStateResponse?> GetGameStateAsync(Guid gameId);

    // Applies one match result to the bracket engine.
    Task<bool> ReportMatchResultAsync(Guid gameId, ReportMatchRequest request);

    // Submits one authenticated player's vote for the current active match winner.
    Task<SubmitMatchVoteResponse> SubmitMatchVoteAsync(Guid gameId, string voterUserId, SubmitMatchVoteRequest request);
}