namespace Services;

using System.Security.Claims;
using Contracts;
using Entities;

// Defines game lifecycle and bracket progression operations used by API routes.
public interface IGameService
{
    // Creates a new game using default settings.
    Task<Guid> CreateGame();

    // Creates a new game with explicit bracket options.
    Task<Guid> CreateGame(CreateGameOptions options);

    // Ends an existing game and clears its runtime state.
    bool EndGame(Guid endGameId);

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