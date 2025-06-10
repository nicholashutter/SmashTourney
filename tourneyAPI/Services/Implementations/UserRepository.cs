
namespace Services;

/* UserRepository implements a repository layer for User Objects to persist them to the database */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Serilog;

public class UserRepository : IUserRepository
{

    private readonly AppDBContext _db;

    public UserRepository(AppDBContext db)
    {
        _db = db;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid id)
    {
        Log.Information($"Info: Get User By Id {id}");

        var foundUser = new ApplicationUser();
        try
        {
            foundUser = await _db.Users.FindAsync(id);

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

    public async Task<Guid?> CreateUserAsync(ApplicationUser newUser)
    {

        Log.Information("Info: Create User Async");

        try
        {
            if (newUser is null)
            {
                throw new UserNotFoundException("CreateUserAsync");
            }
            else
            {
                await _db.AddAsync(newUser);
                await _db.SaveChangesAsync();

                return newUser.Id;
            }
        }
        catch (UserNotFoundException e)
        {
            Log.Error($"Error: newUser is null. Unable to create newUser \n {e}");
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

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        Log.Information("Info: Update User Async");

        var foundUser = new ApplicationUser();

        try
        {

            foundUser = await _db.Users.FindAsync(id);

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