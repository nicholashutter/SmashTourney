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
    Task<IEnumerable<Game>?> GetAllGamesAsync();

    //api route StartGame
    Task<bool> StartGameAsync(Guid existingGameId);

    //api route //StartRound
    Task<List<Player>?> StartRound(Guid gameId);

    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    Task<List<Player>?> StartMatch(Guid gameId);

    //api route EndMatch
    Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser);

    //api route /EndRound
    Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser);
    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    Task<Game?> LoadGameAsync(Guid gameId);
    //api route /SaveGame
    //SaveGame will persist current game state to the database
    Task<bool> SaveGameAsync(Guid gameId);

    //called by StartGame
    Task<bool> GenerateBracketAsync(Guid gameId);

    //api route /AllPlayersIn
    Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId);

    //Api route /AddUserToLobby
    Task<bool> AddUserToLobby(User addUser, Guid gameId);

    //called by SaveGameAsync or EndGameAsync
    Task<bool> UpdateUserScore(Guid gameId);

    //called by endRoundAsync
    Task<bool> VoteHandlerAsync(Guid gameId, Player RoundWinner, Player RoundLoser);

}