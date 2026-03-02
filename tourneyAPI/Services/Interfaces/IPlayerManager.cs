namespace Services;

using Entities;

// Defines player repository operations used by player and game flows.
public interface IPlayerManager
{
    // Creates and stores a new player record.
    Task<Guid?> CreateAsync(Player newPlayer);

    // Returns one player by identifier.
    Task<Player?> GetByIdAsync(Guid id);

    // Returns all players.
    Task<List<Player>?> GetAllPlayersAsync();

    // Updates an existing player record.
    Task<bool> UpdateAsync(Player updatePlayer);

    // Deletes a player record by identifier.
    Task<bool> DeleteAsync(Guid id);

}