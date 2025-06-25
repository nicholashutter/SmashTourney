
namespace Services;

/* UserRepository implements a repository layer for User Objects to persist them to the database */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class UserRepository : IUserRepository
{

    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<string> CreateUserAsync(ApplicationUser user)
    {
        Task<string> result = Task.Run(async () =>
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

        });

        return result;
    }

    public Task<ApplicationUser?> GetUserByIdAsync(string Id)
    {


        Task<ApplicationUser?> result = Task.Run(async () =>
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

        });

        return result;
    }

    public Task<List<ApplicationUser>?> GetAllUsersAsync()
    {

        Task<List<ApplicationUser>?> result = Task.Run(async () =>
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
        });

        return result;
    }


    public Task<bool> UpdateUserAsync(ApplicationUser updateUser)
    {
        Task<bool> result = Task.Run(async () =>
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
        });

        return result;
    }

    public Task<bool> DeleteUserAsync(string Id)
    {
        Task<bool> result = Task.Run(async () =>
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
        });
        return result;
    }
}