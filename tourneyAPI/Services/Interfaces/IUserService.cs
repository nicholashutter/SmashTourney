namespace Services;

using Entities;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User newUser);
    Task<bool> UpdateUserAsync(User updateUser);
    Task<bool> DeleteUserAsync(Guid id);

}