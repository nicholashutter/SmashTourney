using System;
using Xunit;
using Validators;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace Tests;

public class UserRepositoryTest
{
    AppDBContext db;

    UserRepository repository;
    public UserRepositoryTest()
    {
        db = new AppDBContext();
        repository = new UserRepository(db);
    }

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public async Task testCreateUserAsync()
    {

        var success = await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        Assert.IsType<Guid>(success);
    }

    [Fact]
    public async Task getAllUsersNotNull()
    {
        await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        await repository.CreateUserAsync(new User
        {
            Username = "jason",
            Email = "jason.micheal@email.com"
        });

        var users = await repository.GetAllUsersAsync();

        Assert.NotNull(users);
    }

    [Fact]
    public async Task getAllUsersAccurateUsername()
    {
        var success = await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        success = await repository.CreateUserAsync(new User
        {
            Username = "jason",
            Email = "jason.micheal@email.com"
        });

        var users = await repository.GetAllUsersAsync();
        Assert.Equal("nicholas", users[0].Username);
    }

    [Fact]
    public async Task getUserById()
    {
        var IdOne = await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var IdTwo = await repository.CreateUserAsync(new User
        {
            Username = "jason",
            Email = "jason.micheal@email.com"
        });

        if (IdOne is not null)
        {
            var user = await repository.GetUserByIdAsync((Guid)IdOne);
            Assert.Equal(IdOne, user.Id);
        }

    }

    [Fact]
    public async Task updateUserAsync()
    {
        var Id = await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var success = false;
        if (Id is not null)
        {
            success = await repository.UpdateUserAsync(new User
            {
                Id = (Guid)Id,
                Username = "nicholasUpdated",
                Email = "nicholasUpdated.hutter@email.com"
            });
        }

        Assert.True(success);
    }

    [Fact]
    public async Task deleteUserAsync()
    {
        var Id = await repository.CreateUserAsync(new User
        {
            Username = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var success = false;
        if (Id is not null)
        {
            success = await repository.UpdateUserAsync(new User
            {
                Id = (Guid)Id,
                Username = "nicholasUpdated",
                Email = "nicholasUpdated.hutter@email.com"
            });
        }

        Assert.True(success);
    }
}

