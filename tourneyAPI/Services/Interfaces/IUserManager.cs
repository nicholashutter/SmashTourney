namespace Services;

using Entities;
using Microsoft.AspNetCore.Identity;

// Defines user account operations used by user routes and setup utilities.
public interface IUserManager
{
    // Creates a user account with the supplied password.
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);

    // Returns one user by identifier.
    Task<ApplicationUser?> GetUserByIdAsync(string Id);

    // Returns all users.
    Task<List<ApplicationUser>?> GetAllUsersAsync();

    // Updates an existing user profile.
    Task<IdentityResult> UpdateUserAsync(ApplicationUser user);

    // Deletes a user by identifier.
    Task<IdentityResult> DeleteUserAsync(string Id);

    // Returns one user by username.
    Task<ApplicationUser?> GetUserByUserNameAsync(string UserName);
}