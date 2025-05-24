namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    Guid CreateGame();
    bool EndGameAsync(Game game);
    Task<Game?> GetGameByIdAsync(Guid Id);
    Task<IEnumerable<Game>?> GetAllGamesAsync();
    Task<bool> StartGameAsync(Game game);
    Task<bool> StartRoundAsync(Game game);
    Task<bool> EndRoundAsync(Game game);
    Task<bool> LoadGameAsync(Game game);
    Task<bool> SaveGameAsync(Game saveGame);
    bool AddLobbyToGame(List<Player> players);
    bool AddUserToLobby(User addUser, Guid gameId);
    Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID);

}