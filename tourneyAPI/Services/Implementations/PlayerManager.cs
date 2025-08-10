namespace Services;


/* PlayerrRepository implements a repository layer for Player Objects to persist them to the database */
/* This class currently has no use and may get cut from the first release */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class PlayerManager : IPlayerManager
{
    private readonly ApplicationDbContext _db;


    public PlayerManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> CreateAsync(Player newPlayer)
    {
        Log.Information("Creating New Player");

        try
        {
            if (newPlayer is null)
            {
                throw new PlayerNotFoundException("CreateAsync");
            }
            else
            {
                await _db.Players.AddAsync(newPlayer);
                await _db.SaveChangesAsync();
                return newPlayer.Id;
            }
        }
        catch (PlayerNotFoundException e)
        {
            Log.Error($"NewPlayer is null. Unable to create newPlayer\n {e}");
            return null;
        }
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        Log.Information("Getting Player By Id {id}", id);

        var foundPlayer = new Player { UserId = "" };
        try
        {
            foundPlayer = await _db.Players.FindAsync(id);

            if (foundPlayer is null)
            {
                throw new PlayerNotFoundException("GetByIdAsync");
            }
            else
            {
                return foundPlayer;
            }
        }
        catch (PlayerNotFoundException e)
        {
            Log.Information($"Error: Player not found or otherwise null\n {e}");
            return null;
        }
    }

    public async Task<List<Player>?> GetAllPlayersAsync()
    {
        Log.Information("Get All Players");

        var Players = new List<Player>();

        try
        {
            Players = await _db.Players.ToListAsync();

            if (Players.Count == 0)
            {
                throw new EmptyPlayersCollectionException("GetAllAsync");
            }
            return Players;
        }
        catch (EmptyPlayersCollectionException e)
        {
            Log.Warning($"All Players Returns Zero, Did You Just Reset The DB? \n {e}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(Player updatePlayer)
    {
        Log.Information("Update Player Async");

        try
        {
            if (updatePlayer == null)
            {
                throw new InvalidArgumentException("UpdateAsync");
            }
            else
            {
                var foundPlayer = await _db.Players.FindAsync(updatePlayer.Id);
                if (foundPlayer is null)
                {
                    throw new PlayerNotFoundException("UpdateAsync");
                }
                else
                {
                    _db.Update(foundPlayer);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }
        }
        catch (PlayerNotFoundException e)
        {
            Log.Error($"UpdatePlayer is null. Unable to updatePlayer\n {e}");
            return false;
        }
        catch (InvalidArgumentException e)
        {
            Log.Error("Error: {e}", e.ToString());
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Log.Information($"Delete Player {id}");

        var foundPlayer = new Player { UserId = "" };

        try
        {
            foundPlayer = await _db.Players.FindAsync(id);

            if (foundPlayer == null)
            {
                throw new PlayerNotFoundException("DeleteAsync");
            }
            else
            {
                _db.Players.Remove(foundPlayer);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (PlayerNotFoundException e)
        {
            Log.Warning($"Player not found. Unable to delete\n {e}");
            return false;
        }
    }
}