namespace Services;

using Entities;
public interface IGameService
{
    Task<bool> CreateNewGameAsync(Game newGame);
    Task<Game?> GetGameByIdAsync(Guid Id);
    Task<IEnumerable<Game>?> GetAllGamesAsync();
    Task<bool> StartGameAsync(Game game);
    Task<bool> StartRoundAsync(Game game);
    Task<bool> EndRoundAsync(Game game);
    Task<bool> EndGameAsync(Game game);
    Task<bool> AddPlayerToGameAsync(Game game, Player player);
    Task<bool> RemovePlayerFromGameAsync(Game game, Guid playerId);
    Task<bool> SaveGameAsync(Game game);
    Task<bool> LoadGameAsync(Game game);
    Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID);

}