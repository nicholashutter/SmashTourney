
namespace Services;

/* UserRepository implements a repository layer for User Objects to persist them to the database */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class UserManager : IUserManager
{

    private readonly ApplicationDbContext _db;

    public UserManager(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> CreateUserAsync(ApplicationUser user)
    {
        try
            {
                await _db.AddAsync(user);
                await _db.SaveChangesAsync();

                return user.Id;
            }
            catch (IOException e)
            {
                Log.Error(e.ToString());
                return user.Id;
            }
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string Id)
    {
        Log.Information($"Info: Get User By Id {Id}");

            var foundUser = new ApplicationUser();
            try
            {
                foundUser = await _db.Users.FindAsync(Id);

                if (foundUser is null)
                {
                    throw new UserNotFoundException("GetUserByIdAsync");
                }
                else
                {
                    return foundUser;
                }

            }
            catch (UserNotFoundException e)
            {
                Log.Information($"Error: User not found or otherwise null\n  {e}");
                return null;
            }
    }

    public async Task<ApplicationUser?> GetUserByUserNameAsync(string UserName)
    {
        Log.Information("Info: Get User By Username");

        var foundUser = new ApplicationUser();
            try
            {
                foundUser = await _db.Users.FindAsync(UserName);

                if (foundUser is null)
                {
                    throw new UserNotFoundException("GetUserByUserNameAsync");
                }
                else
                {
                    return foundUser;
                }

            }
            catch (UserNotFoundException e)
            {
                Log.Information($"Error: User not found or otherwise null\n  {e}");
                return null;
            }
    }

    public async Task<List<ApplicationUser>?> GetAllUsersAsync()
    {

        Log.Information("Info: Get All Users");

        var Users = new List<ApplicationUser>();

        try
        {
            Users = await _db.Users.ToListAsync();

            if (Users.Count == 0)
            {
                throw new EmptyUsersCollectionException("GetAllUsersAsync");
            }
            return Users;
        }
        catch (EmptyUsersCollectionException e)
        {
            Log.Warning($"All Users Returns Zero, Did You Just Reset The DB? \n {e}");
            return null;
        }
    }


    public async Task<bool> UpdateUserAsync(ApplicationUser updateUser)
    {
        Log.Information("Info: Update User Async");

            try
            {
                if (updateUser is null)
                {
                    throw new UserNotFoundException("UpdateUserAsync");
                }
                else
                {
                    var foundUser = await _db.Users.FindAsync(updateUser.Id);

                    if (foundUser is null)
                    {
                        throw new InvalidArgumentException("UpdateUserAsync");
                    }
                    else
                    {
                        _db.Update(foundUser);
                        await _db.SaveChangesAsync();
                        return true;
                    }

                }
            }
            catch (UserNotFoundException e)
            {
                Log.Error($"Error: updateUser is null. Unable to updateUser \n {e}");
                return false;
            }
            catch (InvalidArgumentException e)
            {
                Log.Error($"Error: {e}");
                return false;
            }
    }

    public async Task<bool> DeleteUserAsync(string Id)
    {
         Log.Information("Info: Update User Async");

            var foundUser = new ApplicationUser();

            try
            {

                foundUser = await _db.Users.FindAsync(Id);

                if (foundUser == null)
                {
                    throw new UserNotFoundException("DeleteUserAsync");
                }
                else
                {
                    _db.Users.Remove(foundUser);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }
            catch (UserNotFoundException e)
            {
                Log.Warning($"Warning: User not found. Unable to delete {e}");
                return false;
            }
    }
}