namespace Services;

using Entities;
public interface IPlayerService
{
    Task<bool> CreateAsync(Player newPlayer);
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>?> GetAllAsync();
    Task<bool> UpdateAsync(Player updatePlayer);
    Task<bool> DeleteAsync(Guid id);

}