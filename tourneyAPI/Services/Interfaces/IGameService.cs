namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    // API route /NewGame
    Guid CreateGame();

    // API route /EndGame
    bool EndGame(Guid endGameId);

    // API route /GetGameById (debug)
    Task<Game?> GetGameByIdAsync(Guid gameId);

    // API route /GetAllGames (debug)
    Task<List<Game>?> GetAllGamesAsync();

    // API route /AddUserToLobby
    public bool CreateUserSession(ApplicationUser addUser, Guid gameId);

    // API route /AllPlayersIn
    public bool AddPlayersToGame(List<Player> players, Guid gameId);

    // Called by StartGame
    public bool GenerateBracket(Guid gameId);

    // API route StartGame
    public Task<bool> StartGameAsync(Guid existingGameId);

    // LoadGameAsync will check against the db and attempt to restore
    // game state from games persisted in db
    public Task<bool> LoadGameAsync(Guid gameId);

    // API route /SaveGame
    // SaveGame will persist current game state to the database
    public Task<bool> SaveGameAsync(Guid gameId);

    // Called by SaveGameAsync or EndGameAsync
    public Task<bool> UpdateUserScoreAsync(Guid gameId);

    // Gameservice will need to pass itself to roundService and track an instance of roundService
    // API route //StartRound
    List<Player>? StartRound(Guid gameId);

    // API route /EndRound
    bool EndRound(Guid gameId);

    // API route StartMatch
    List<Player>? StartMatch(Guid gameId);

    // API route EndMatch
    Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser);

    // Called by EndRound
    // May become private
    bool VoteHandler(Guid gameId, Player MatchWinner);

    /*----------------------------------------------------END GAME SERVICE---------------------------------------------------- */
}