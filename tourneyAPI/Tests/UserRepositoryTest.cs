using Xunit;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class UserRepositoryTest
{

    private const string dbName = "tourneydb.db";
    ApplicationDbContext db;

    UserRepository repository;
    public UserRepositoryTest()
    {
        db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>(), "tourneydb.db");
        repository = new UserRepository(db);
    }

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public async Task createUserAsync()
    {

        var success = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        Assert.IsType<Guid>(success);
    }

    [Fact]
    public async Task getAllUsersNotNull()
    {
        await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "jason",
            Email = "jason.micheal@email.com"
        });

        var users = await repository.GetAllUsersAsync();

        Assert.NotNull(users);
    }

    [Fact]
    public async Task getAllUsersAccurateUsername()
    {
        var success = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        success = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "jason",
            Email = "jason.micheal@email.com"
        });

        var users = await repository.GetAllUsersAsync();
        Assert.Equal("nicholas", users[0].UserName);
    }

    [Fact]
    public async Task getUserById()
    {
        var IdOne = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var IdTwo = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "jason",
            Email = "jason.micheal@email.com"
        });

        if (IdOne is not null)
        {
            var user = await repository.GetUserByIdAsync((Guid)IdOne);
            Assert.Equal(IdOne, user.Id);
        }
        else
        {
            throw new Exception();
        }

    }

    [Fact]
    public async Task updateUserAsync()
    {
        var Id = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var success = false;
        if (Id is not null)
        {
            success = await repository.UpdateUserAsync(new ApplicationUser
            {
                Id = (Guid)Id,
                UserName = "nicholasUpdated",
                Email = "nicholasUpdated.hutter@email.com"
            });
        }
        else
        {
            throw new Exception();
        }

        Assert.True(success);
    }

    [Fact]
    public async Task deleteUserAsync()
    {
        var Id = await repository.CreateUserAsync(new ApplicationUser
        {
            UserName = "nicholas",
            Email = "nicholas.hutter@email.com"
        });

        var success = false;
        if (Id is not null)
        {
            success = await repository.UpdateUserAsync(new ApplicationUser
            {
                Id = (Guid)Id,
                UserName = "nicholasUpdated",
                Email = "nicholasUpdated.hutter@email.com"
            });
        }
        else
        {
            throw new Exception();
        }

        Assert.True(success);
    }
}

