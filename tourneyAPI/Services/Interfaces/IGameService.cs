namespace Services;

using Entities;
public interface IGameService
{
    Task<Game> CreateNewGameAsync(Game newGame);
    Task<Game> GetGameByIdAsync(Guid Id);


    Task StartGameAsync(Game game);
    Task StartRoundAsync(Game game);
    Task EndRoundAsync(Game game);
    Task EndGameAsync(Game game);
    Task<bool> AddPlayerToGameAsync(Game game, Player player);
    Task<bool> RemovePlayerFromGameAsync(Game game, Guid playerId);
    Task SaveGameAsync(Game game);
    Task LoadGameAsync(Game game);
    Task VoteHandlerAsync(Guid playerID, Guid playerToVoteForID);

}