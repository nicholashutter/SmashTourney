namespace Services;

using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Implements persistence operations for player records.
public class PlayerManager : IPlayerManager
{
    private readonly ApplicationDbContext _db;

    // Creates a new player manager with the application database context.
    public PlayerManager(ApplicationDbContext db)
    {
        _db = db;
    }

    // Creates a new player record.
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

    // Gets one player record by identifier.
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

    // Gets all player records.
    public async Task<List<Player>?> GetAllPlayersAsync()
    {
        Log.Information("Get All Players");

        var players = new List<Player>();

        try
        {
            players = await _db.Players.ToListAsync();

            if (players.Count == 0)
            {
                throw new EmptyPlayersCollectionException("GetAllAsync");
            }
            return players;
        }
        catch (EmptyPlayersCollectionException e)
        {
            Log.Warning($"All Players Returns Zero, Did You Just Reset The DB? \n {e}");
            return null;
        }
    }

    // Updates an existing player record.
    public async Task<bool> UpdateAsync(Player updatePlayer)
    {
        Log.Information("Update Player Async");
        if (updatePlayer?.Id == null)
        {
            Log.Error("UpdatePlayer is null");
            return false;
        }
        var foundPlayer = await _db.Players.FindAsync(updatePlayer.Id);
        if (foundPlayer == null)
        {
            Log.Error("Player not found during update");
            return false;
        }
        _db.Update(foundPlayer);
        await _db.SaveChangesAsync();
        return true;
    }

    // Deletes a player record by identifier.
    public async Task<bool> DeleteAsync(Guid id)
    {
        Log.Information($"Delete Player {id}");
        var foundPlayer = await _db.Players.FindAsync(id);
        if (foundPlayer == null)
        {
            Log.Warning("Player not found. Unable to delete");
            return false;
        }
        _db.Players.Remove(foundPlayer);
        await _db.SaveChangesAsync();
        return true;
    }
}