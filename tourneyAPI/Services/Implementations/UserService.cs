
namespace Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ILogger<GameService> _logger;

    private readonly AppDBContext _db;

    public UserService(ILogger<GameService> logger, AppDBContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Info: Get User By Id {id}", id);

        var foundUser = new User();
        try
        {
            foundUser = await _db.Users.FindAsync(id);

            if (foundUser is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return foundUser;
            }

        }
        catch (NullReferenceException e)
        {
            _logger.LogInformation("Error: User not found or otherwise null\n  {e}", e.ToString());
            return foundUser;
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Info: Get All Users");

        var Users = new List<User>();

        try
        {
            Users = await _db.Users.ToListAsync();

            if (Users.Count == 0)
            {
                throw new NullReferenceException();
            }
            return Users;
        }
        catch (NullReferenceException e)
        {
            _logger.LogWarning("All Users Returns Zero, Did You Just Reset The DB?");
            return Users;
        }
    }

    public async Task<bool> CreateUserAsync(User newUser)
    {

        _logger.LogInformation("Info: Create User Async");

        try
        {
            if (newUser is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                await _db.AddAsync(newUser);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: newUser is null. Unable to create newUser");
            return false;
        }

    }

    public async Task<bool> UpdateUserAsync(User updateUser)
    {
        _logger.LogInformation("Info: Update User Async");

        try
        {
            if (updateUser == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                await _db.AddAsync(updateUser);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: updateUser is null. Unable to updateUser");
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
                throw new NullReferenceException();
            }
            else
            {
                _db.Users.Remove(foundUser);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogWarning("Warning: User not found. Unable to delete");
            return false;
        }
    }
}