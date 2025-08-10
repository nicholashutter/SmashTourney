
namespace Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using Helpers;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Serilog;

/* UserManager is an identityUser service that implements repository pattern */ 
public class UserManager : IUserManager
{
    private readonly UserManager<ApplicationUser> _identityUserManager;

    public UserManager(UserManager<ApplicationUser> identityUserManager)
    {
        _identityUserManager = identityUserManager;
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _identityUserManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Log.Error($"Identity user creation error: Code={error.Code}, Description={error.Description}");
            }
        }

        return result;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id)
    {
        Log.Information("Attempting to retrieve user by ID: {UserId}", id);

        if (id.Equals(AppConstants.ByeUserId))
        {
            Log.Information("Bye User ID '{ByeUserId}' processed. Will not be found in database.", AppConstants.ByeUserId);
            return null;
        }

        var foundUser = await _identityUserManager.FindByIdAsync(id);

        if (foundUser == null)
        {
            Log.Warning("User with ID '{UserId}' not found.", id);
        }

        return foundUser;
    }


    public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
    {
        Log.Information("Attempting to retrieve user by username: {UserName}", userName);

        var foundUser = await _identityUserManager.FindByNameAsync(userName);

        if (foundUser == null)
        {

            Log.Warning("User with username '{UserName}' not found.", userName);
        }

        return foundUser;
    }

    public async Task<List<ApplicationUser>?> GetAllUsersAsync()
    {
        Log.Information("Attempting to retrieve all users.");

        var allUsers = await _identityUserManager.Users.ToListAsync();

        if (allUsers.Count == 0)
        {

            Log.Information("No users found in the database.");
        }

        return allUsers;
    }



    public async Task<IdentityResult> UpdateUserAsync(ApplicationUser updateUser)
    {
        Log.Information("Attempting to update user with ID: {UserId}", updateUser?.Id);

        if (updateUser == null)
        {
            Log.Warning("UpdateUserAsync received a null ApplicationUser object.");
            return IdentityResult.Failed(new IdentityError { Description = "Update user object cannot be null." });
        }

        var existingUser = await _identityUserManager.FindByIdAsync(updateUser.Id);

        if (existingUser == null)
        {
            Log.Warning("User with ID '{UserId}' not found for update.", updateUser.Id);
            return IdentityResult.Failed(new IdentityError { Description = $"User with ID '{updateUser.Id}' not found for update." });
        }

        existingUser.Email = updateUser.Email;
        existingUser.UserName = updateUser.UserName;
        existingUser.RegistrationDate = updateUser.RegistrationDate;
        existingUser.LastLoginDate = updateUser.LastLoginDate;
        existingUser.AllTimeMatches = updateUser.AllTimeMatches;
        existingUser.AllTimeWins = updateUser.AllTimeWins;
        existingUser.AllTimeLosses = updateUser.AllTimeLosses;

        var result = await _identityUserManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Log.Error("User update failed for ID '{UserId}': Code={ErrorCode}, Description={ErrorDescription}",
                    updateUser.Id, error.Code, error.Description);
            }
        }
        return result;
    }

    public async Task<IdentityResult> DeleteUserAsync(string id)
    {
        Log.Information("Attempting to delete user with ID: {UserId}", id);

        if (string.IsNullOrEmpty(id))
        {
            Log.Warning("DeleteUserAsync received a null or empty user ID.");
            return IdentityResult.Failed(new IdentityError { Description = "User ID cannot be null or empty." });
        }

        var userToDelete = await _identityUserManager.FindByIdAsync(id);

        if (userToDelete == null)
        {
            Log.Warning("User with ID '{UserId}' not found for deletion.", id);
            return IdentityResult.Failed(new IdentityError { Description = $"User with ID '{id}' not found for deletion." });
        }

        var result = await _identityUserManager.DeleteAsync(userToDelete);

        if (!result.Succeeded)
        {

            foreach (var error in result.Errors)
            {
                Log.Error("User deletion failed for ID '{UserId}': Code={ErrorCode}, Description={ErrorDescription}",
                    id, error.Code, error.Description);
            }
        }

        return result;
    }

}