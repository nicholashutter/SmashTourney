
namespace Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;

public class UserService : IUserService
{
    public Task<User> GetUserByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> CreateUserAsync(User newUser)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserAsync(User updateUser)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}