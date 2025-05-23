
namespace Services;

using Entities;
using System;
using System.Threading.Tasks;

public class GameService : IGameService
{
    public Task<Game> CreateNewGameAsync(Game newGame)
    {
        throw new NotImplementedException();
    }

    public Task<Game> GetGameByIdAsync(Guid Id)
    {
        throw new NotImplementedException();
    }

    public Task StartGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task StartRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task EndRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task EndGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddPlayerToGameAsync(Game game, Player player)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemovePlayerFromGameAsync(Game game, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task SaveGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task LoadGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task VoteHandlerAsync(Guid playerID, Guid playerToVoteForID)
    {
        throw new NotImplementedException();
    }
}