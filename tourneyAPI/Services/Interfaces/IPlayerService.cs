namespace Services;

using Entities;
public interface IPlayerService
{
    Task<Player> CreateAsync(Player newPlayer);
    Task<Player> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player> UpdateAsync(Player updatePlayer);
    Task<bool> DeleteAsync(Guid id);

}