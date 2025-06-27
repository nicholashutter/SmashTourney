namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    //api route /NewGame 
    Task<Guid> CreateGame();

    //api route /EndGame
    Task<bool> EndGameAsync(Guid gameId);

    //api route /GetGameById (debug)
    Task<Game?> GetGameByIdAsync(Guid getId);

    //api route /GetAllGames (debug)
    Task<List<Game>?> GetAllGamesAsync();

    //api route StartGame
    Task<bool> StartGameAsync(Guid existingGameId);

    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    Task<bool> LoadGameAsync(Guid gameId);
    //api route /SaveGame
    //SaveGame will persist current game state to the database
    Task<bool> SaveGameAsync(Guid gameId);
    
        //called by StartGame
    Task<bool> GenerateBracketAsync(Guid gameId);

    //api route /AllPlayersIn
    Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId);

    //Api route /AddUserToLobby
    Task<bool> AddUserToLobby(ApplicationUser addUser, Guid gameId);

    //called by SaveGameAsync or EndGameAsync
    Task<bool> UpdateUserScore(Guid gameId);

    /*----------------------------------------------------END GAME SERVICE---------------------------------------------------- */




}