namespace Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging; 

public class PlayerService : IPlayerService
{
    private readonly AppDBContext _db;
    private readonly ILogger<PlayerService> _logger; 

   
    public PlayerService(AppDBContext db, ILogger<PlayerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(Player newPlayer)
    {
        _logger.LogInformation("Info: Create Player Async");

        try
        {
            if (newPlayer is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                await _db.Players.AddAsync(newPlayer); 
                await _db.SaveChangesAsync();
                return true; 
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: newPlayer is null. Unable to create newPlayer\n {e}", e.ToString());
            return false; 
        }
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Info: Get Player By Id {id}", id);

        var foundPlayer = new Player();
        try
        {
            foundPlayer = await _db.Players.FindAsync(id);

            if (foundPlayer is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return foundPlayer;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogInformation("Error: Player not found or otherwise null\n {e}", e.ToString());
            return null;
        }
    }

    public async Task<IEnumerable<Player>?> GetAllAsync()
    {
        _logger.LogInformation("Info: Get All Players");

        var Players = new List<Player>();

        try
        {
            Players = await _db.Players.ToListAsync();

            if (Players.Count == 0)
            {
                throw new NullReferenceException();
            }
            return Players;
        }
        catch (NullReferenceException e)
        {
            _logger.LogWarning("All Players Returns Zero, Did You Just Reset The DB?");
            return null; 
        }
    }

    public async Task<bool> UpdateAsync(Player updatePlayer) 
    {
        _logger.LogInformation("Info: Update Player Async");

        try
        {
            if (updatePlayer == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                var foundPlayer = await _db.Users.FindAsync(updatePlayer.Id);
                if (foundPlayer is null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    _db.Update(foundPlayer);
                    await _db.SaveChangesAsync();
                    return true; 
                }
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: updatePlayer is null. Unable to updatePlayer\n {e}", e.ToString());
            return false; 
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Info: Delete Player Async"); 

        var foundPlayer = new Player(); 

        try
        {
            foundPlayer = await _db.Players.FindAsync(id);

            if (foundPlayer == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                _db.Players.Remove(foundPlayer);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogWarning("Warning: Player not found. Unable to delete\n {e}", e.ToString());
            return false;
        }
    }
}