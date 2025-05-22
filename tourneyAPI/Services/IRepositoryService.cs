namespace Services;

using DTO;
using Entities;

public interface IRepositoryService
{
    Task AddUserAsync(UserDTO userDTO);
    Task<UserDTO?> GetUserByIdAsync(Guid id);
    Task<UserDTO?> GetUserByUsernameAsync(string username);
    Task UpdateUserAsync(UserDTO userDTO);
    Task DeleteUserAsync(Guid id);

    Task AddGameAsync(GameDTO gameDTO);
    Task<GameDTO?> GetGameByIdAsync(Guid sessionId);
    Task UpdateGameAsync(GameDTO gameDTO);
    Task DeleteGameAsync(Guid sessionId);

    Task SaveChangesAsync();

}