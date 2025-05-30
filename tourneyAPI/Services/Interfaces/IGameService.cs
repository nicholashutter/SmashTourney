namespace Services;

using System.Security;
using Entities;
using Microsoft.Extensions.Configuration.UserSecrets;

public interface IGameService
{
    Task<Guid> CreateGame();
    Task<bool> EndGameAsync(Guid gameId);
    Task<Game?> GetGameByIdAsync(Guid getId);
    Task<IEnumerable<Game>?> GetAllGamesAsync();
    Task<bool> StartGameAsync(Guid existingGameId);

    Task<List<Player>?> StartRound(Guid gameId);

    Task<List<Player>?> StartMatch(Guid gameId);
    Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser);
    Task<Game?> LoadGameAsync(Guid gameId);
    Task<bool> SaveGameAsync(Guid gameId);

    Task<bool> GenerateBracketAsync(Guid gameId);
    Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId);
    Task<bool> AddUserToLobby(User addUser, Guid gameId);
    Task<bool> UpdateUserScore(Guid gameId);

    Task<bool> VoteHandlerAsync(Guid gameId, Player RoundWinner, Player RoundLoser);

}