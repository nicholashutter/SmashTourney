namespace Services;

using DTO;
using Entities;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid id);
    Task<User> GetUserByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(UserDTO userDTO);
    Task<bool> UpdateUserAsync(Guid id, UserDTO userDTO);
    Task<bool> DeleteUserAsync(Guid id);

}