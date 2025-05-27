namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    Task<Guid> CreateGame();
    Task<bool> EndGameAsync(Guid gameId);
    Task<Game?> GetGameByIdAsync(Guid Id);
    Task<IEnumerable<Game>?> GetAllGamesAsync();
    Task<bool> StartGameAsync(Guid existingGameId);
    Task<bool> StartRoundAsync(Game currentGame);

    /*
    Task<bool> EndRoundAsync(Game game); */ 
    Task<bool> LoadGameAsync(Game loadGame);
    Task<bool> SaveGameAsync(Game saveGame);

    Task<bool> GenerateBracketAsync(Guid gameId);

    Task<bool> UpdateBracketAsync(Guid gameId); 

    Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId);
    Task<bool> AddUserToLobby(User addUser, Guid gameId);
    Task<bool> UpdatePlayerScoreAsync(Guid gameId, Player RoundWinner, Player RoundLoser);

    Task<bool> UpdateUserScore(Guid gameId);

    Task<bool> VoteHandlerAsync(Guid playerID, Player RoundWinner, Player RoundLoser);

}