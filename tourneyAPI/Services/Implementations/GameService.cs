namespace Services;

using System;
using System.Collections.Generic;
using System.Data.Common; 
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging; 
public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly AppDBContext _db;
    public GameService(ILogger<GameService> logger, AppDBContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<bool> CreateNewGameAsync(Game newGame)
    {
        _logger.LogInformation("Info: Create New Game Async");

        try
        {
            if (newGame is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                await _db.Games.AddAsync(newGame);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: newGame is null. Unable to create new game\n {e}", e.ToString());
            return false;
        }
    }

    public async Task<Game?> GetGameByIdAsync(Guid Id)
    {
        _logger.LogInformation("Info: Get Game By Id {Id}", Id);

        var foundGame = new Game(); 
        try
        {
            foundGame = await _db.Games.FindAsync(Id);

            if (foundGame is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return foundGame;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogInformation("Error: Game not found or otherwise null\n {e}", e.ToString());
            return null;
        }
    }

    public async Task<IEnumerable<Game>?> GetAllGamesAsync()
    {
        _logger.LogInformation("Info: Get All Games");

        var Games = new List<Game>(); 

        try
        {
            Games = await _db.Games.ToListAsync();

            if (Games.Count == 0)
            {
                throw new NullReferenceException();
            }
            return Games;
        }
        catch (NullReferenceException e)
        {
            _logger.LogWarning("All Games Returns Zero, Did You Just Reset The DB?");
            return null;
        }
    }

    public Task<bool> StartGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EndRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EndGameAsync(Game game)
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

    public Task<bool> SaveGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LoadGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID)
    {
        throw new NotImplementedException();
    }
}