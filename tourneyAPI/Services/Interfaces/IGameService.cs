namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    Guid CreateGame();
    bool EndGameAsync(Guid gameId);
    Task<Game?> GetGameByIdAsync(Guid Id);
    Task<IEnumerable<Game>?> GetAllGamesAsync();
    Task<bool> StartGameAsync();
    Task<bool> StartRoundAsync(Game game);
    Task<bool> EndRoundAsync(Game game);
    Task<bool> LoadGameAsync(Game loadGame);
    Task<bool> SaveGameAsync(Game saveGame);

    Task<bool> GenerateBracket(Guid gameId);

    Task<bool> AddPlayersToGame(List<Player> players, Guid gameId);
    Task<bool> AddUserToLobby(User addUser, Guid gameId);
    Task<bool> UpdatePlayerScore(Guid gameId, Player RoundWinner, Player RoundLoser);

    Task<bool> UpdateUserScore(Guid gameId); 

    Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID);

}