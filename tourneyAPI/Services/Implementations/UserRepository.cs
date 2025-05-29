
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

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;

    private readonly AppDBContext _db;

    public UserRepository(ILogger<UserRepository> logger, AppDBContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation($"Info: Get User By Id {id}");

        var foundUser = new User();
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
            _logger.LogInformation($"Error: User not found or otherwise null\n  {e}");
            return null;
        }
    }

    public async Task<IEnumerable<User>?> GetAllUsersAsync()
    {
        _logger.LogInformation("Info: Get All Users");

        var Users = new List<User>();

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
            _logger.LogWarning($"All Users Returns Zero, Did You Just Reset The DB? \n {e}");
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(User newUser)
    {

        _logger.LogInformation("Info: Create User Async");

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
                return true;
            }
        }
        catch (UserNotFoundException e)
        {
            _logger.LogError($"Error: newUser is null. Unable to create newUser \n {e}");
            return false;
        }

    }

    public async Task<bool> UpdateUserAsync(User updateUser)
    {
        _logger.LogInformation("Info: Update User Async");

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
            _logger.LogError($"Error: updateUser is null. Unable to updateUser \n {e}");
            return false;
        }
        catch (InvalidArgumentException e)
        {
            _logger.LogError($"Error: {e}");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("Info: Update User Async");

        var foundUser = new User();

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
            _logger.LogWarning("Warning: User not found. Unable to delete");
            return false;
        }
    }
}