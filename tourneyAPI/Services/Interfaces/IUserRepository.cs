namespace Services;

using Entities;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUserByIdAsync(string Id);
    Task<List<ApplicationUser>?> GetAllUsersAsync();
    Task<string?> CreateUserAsync(ApplicationUser newUser);
    Task<bool> UpdateUserAsync(ApplicationUser updateUser);
    Task<bool> DeleteUserAsync(string Id);

}