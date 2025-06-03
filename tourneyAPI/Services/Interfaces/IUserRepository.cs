namespace Services;

using Entities;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<List<User>?> GetAllUsersAsync();
    Task<Guid?> CreateUserAsync(User newUser);
    Task<bool> UpdateUserAsync(User updateUser);
    Task<bool> DeleteUserAsync(Guid id);

}