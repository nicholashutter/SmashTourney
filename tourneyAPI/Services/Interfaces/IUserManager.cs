namespace Services;

using Entities;
using Microsoft.AspNetCore.Identity;

public interface IUserManager
{
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<ApplicationUser?> GetUserByIdAsync(string Id);
    Task<List<ApplicationUser>?> GetAllUsersAsync();
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    Task<IdentityResult> DeleteUserAsync(string Id);
    Task<ApplicationUser?> GetUserByUserNameAsync(string UserName);
}