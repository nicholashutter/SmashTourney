namespace Services;

using Entities;

public interface IUserManager
{
    Task<string> CreateUserAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserByIdAsync(string Id);
    Task<List<ApplicationUser>?> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(ApplicationUser user);
    Task<bool> DeleteUserAsync(string Id);
    Task<ApplicationUser?> GetUserByUserNameAsync(string UserName);
}