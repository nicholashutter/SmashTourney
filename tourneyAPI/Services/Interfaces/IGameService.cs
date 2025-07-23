namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    // API route /NewGame
    Task<Guid> CreateGame();

    // API route /EndGame
    bool EndGame(Guid endGameId);

    // API route /GetGameById (debug)
    Task<Game?> GetGameByIdAsync(Guid gameId);

    // API route /GetAllGames (debug)
    Task<List<Game>?> GetAllGamesAsync();

    // API route /AddUserToLobby
    //Also called by callback when ApplicationUser successfully Auths into app
    bool CreateUserSession(ApplicationUser addUser);

    // API route /AllPlayersIn
    bool AddPlayersToGame(List<Player> players, Guid gameId);

    // API route StartGame
    Task<bool> StartGameAsync(Guid existingGameId);

    // LoadGameAsync will check against the db and attempt to restore
    // game state from games persisted in db
    Task<bool> LoadGameAsync(Guid gameId);

    // API route /SaveGame
    // SaveGame will persist current game state to the database
    Task UpdateGameAsync(Guid gameId);

    //API route /NewGame
    Task InsertGameAsync(Game currentGame);

    // Gameservice will need to pass itself to roundService and track an instance of roundService
    // API route //StartRound
    List<Player>? StartRound(Guid gameId);

    // API route /EndRound
    bool EndRound(Guid gameId);

    // API route StartMatch
    List<Player>? StartMatch(Guid gameId);

    // API route EndMatch
    Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser);

    /*----------------------------------------------------END GAME SERVICE---------------------------------------------------- */
}