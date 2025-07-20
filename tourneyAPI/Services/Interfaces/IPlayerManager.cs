namespace Services;

using Entities;
public interface IPlayerManager
{
    Task<Guid?> CreateAsync(Player newPlayer);
    Task<Player?> GetByIdAsync(Guid id);
    Task<List<Player>?> GetAllAsync();
    Task<bool> UpdateAsync(Player updatePlayer);
    Task<bool> DeleteAsync(Guid id);

}